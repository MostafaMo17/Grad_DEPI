using MediCare.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> RegisterPatientAsync(RegisterPatientDto dto);
        Task<LoginResponseDto> RegisterDoctorAsync(RegisterDoctorDto dto);

        // Checks the email and password and returns a token. Throws an exception when the login is not allowed
        Task<LoginResponseDto> LoginAsync(LoginDto dto);

        // Called on every request that carries a token
        // Returns the user current data, or null when the token must no longer be accepted
        Task<UserDto?> GetUserForTokenAsync(int userId);
    }
}
