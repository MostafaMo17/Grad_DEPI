using MediCare.Services.Dtos;
using MediCare.Services.Entities;
using MediCare.Services.Enums;
using MediCare.Services.Exceptions;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly ISpecializationRepository _specializationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            ISpecializationRepository specializationRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _specializationRepository = specializationRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        //  Register 

        public async Task<LoginResponseDto> RegisterPatientAsync(RegisterPatientDto dto)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dto.DateOfBirth == default(DateOnly) || dto.DateOfBirth > today)
            {
                throw new BadRequestException("Date of birth is not valid.");
            }

            ApplicationUser user = CreateUserObject(dto.FullName, dto.Email, dto.PhoneNumber);

            // The profile is saved together with the user in the same SaveChanges
            user.Patient = new Patient
            {
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender
            };

            await CreateAccountAsync(user, dto.Password, RoleNames.Patient);

            return await CreateLoginResponseAsync(user, RoleNames.Patient);
        }

        public async Task<LoginResponseDto> RegisterDoctorAsync(RegisterDoctorDto dto)
        {
            bool specializationExists = await _specializationRepository.ExistsAsync(dto.SpecializationId);
            if (!specializationExists)
            {
                throw new BadRequestException("The selected specialization does not exist.");
            }

            ApplicationUser user = CreateUserObject(dto.FullName, dto.Email, dto.PhoneNumber);

            // new doctor always Pending until Admin approve
            user.Doctor = new Doctor
            {
                SpecializationId = dto.SpecializationId,
                LicenseNumber = dto.LicenseNumber.Trim(),
                ConsultationFee = dto.ConsultationFee,
                ApprovalStatus = DoctorApprovalStatus.Pending
            };

            await CreateAccountAsync(user, dto.Password, RoleNames.Doctor);

            return await CreateLoginResponseAsync(user, RoleNames.Doctor);
        }

        //  Login 

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {

            const string invalidLoginMessage = "Invalid email or password.";
            const string lockedMessage = "Too many failed login attempts. Please try again in a few minutes.";

            ApplicationUser? user = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (user == null)
            {
                throw new UnauthorizedException(invalidLoginMessage);
            }

            // locked account cannot log in, even with the correct password
            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new LockedException(lockedMessage);
            }

            // password is not trimmed, a space can be part of it
            bool passwordIsCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordIsCorrect)
            {
                // This count the failed attempt, After 5 failures Identity locks the account for 5 minutes
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    throw new LockedException(lockedMessage);
                }

                throw new UnauthorizedException(invalidLoginMessage);
            }

            // The password is correct, so clear the failed attempts counter
            await _userManager.ResetAccessFailedCountAsync(user);

            // The account was deactivated by an Admin, We say after the password was proven correct
            if (!user.IsActive)
            {
                throw new ForbiddenException("This account is not active. Please contact the clinic.");
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);
            if (roles.Count != 1)
            {
                // This should never happen (one role per account), It becomes a 500 error and is written to the log
                throw new InvalidOperationException(
                    "User " + user.Id + " must have exactly one role but has " + roles.Count + ".");
            }

            return await CreateLoginResponseAsync(user, roles[0]);
        }

        //  Token check (runs on every request that has a token) 

        public async Task<UserDto?> GetUserForTokenAsync(int userId)
        {
            // The token lives for days, so we check the database on every request
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());

            // User deleted or deactivated: the token is rejected immediately
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // The role always comes from the database, never from the token so a changed role takes effect at once
            IList<string> roles = await _userManager.GetRolesAsync(user);
            if (roles.Count != 1)
            {
                return null;
            }

            return await BuildUserDtoAsync(user, roles[0]);
        }

        //  Helpers 

        private ApplicationUser CreateUserObject(string fullName, string email, string phoneNumber)
        {
            ApplicationUser user = new ApplicationUser();
            user.FullName = fullName.Trim();
            user.Email = email.Trim();
            user.UserName = email.Trim();   // email is the user name
            user.PhoneNumber = phoneNumber.Trim();
            user.IsActive = true;
            return user;
        }

        // Creates the account with its profile and assigns the role in ONE transaction
        // If anything fails, everything is rolled back so never keep a half registered account
        private async Task CreateAccountAsync(ApplicationUser user, string password, string role)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                IdentityResult createResult = await _userManager.CreateAsync(user, password);
                ThrowIfFailed(createResult);

                IdentityResult roleResult = await _userManager.AddToRoleAsync(user, role);
                ThrowIfFailed(roleResult);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // Turn a failed Identity result into our exceptions
        private void ThrowIfFailed(IdentityResult result)
        {
            if (result.Succeeded)
            {
                return;
            }

            foreach (IdentityError error in result.Errors)
            {
                if (error.Code == "DuplicateEmail" || error.Code == "DuplicateUserName")
                {
                    throw new ConflictException("An account with this email already exists.");
                }
            }

            // For example: Passwords must have at least one uppercase
            string message = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(message);
        }

        private async Task<LoginResponseDto> CreateLoginResponseAsync(ApplicationUser user, string role)
        {
            string token = _tokenService.CreateToken(user, out DateTime expiresAtUtc);

            LoginResponseDto response = new LoginResponseDto();
            response.Token = token;
            response.ExpiresAtUtc = expiresAtUtc;
            response.User = await BuildUserDtoAsync(user, role);
            return response;
        }

        // the user data, including the profile ids that the API uses to check ownership
        private async Task<UserDto> BuildUserDtoAsync(ApplicationUser user, string role)
        {
            UserDto dto = new UserDto();
            dto.UserId = user.Id;
            dto.FullName = user.FullName;
            dto.Email = user.Email ?? string.Empty;
            dto.Role = role;

            if (role == RoleNames.Patient)
            {
                Patient? patient = await _patientRepository.GetByUserIdAsync(user.Id);
                if (patient != null)
                {
                    dto.PatientId = patient.PatientId;
                }
            }
            else if (role == RoleNames.Doctor)
            {
                Doctor? doctor = await _doctorRepository.GetByUserIdAsync(user.Id);
                if (doctor != null)
                {
                    dto.DoctorId = doctor.DoctorId;
                    dto.DoctorApprovalStatus = doctor.ApprovalStatus.ToString();
                }
            }

            return dto;
        }
    }
}
