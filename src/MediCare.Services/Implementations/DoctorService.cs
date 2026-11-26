
using MediCare.Services.Dtos;
using MediCare.Services.Enums;
using MediCare.Services.Exceptions;
using MediCare.Services.Interfaces;

namespace MediCare.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(
            IDoctorRepository doctorRepository,
            IUnitOfWork unitOfWork)
        {
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<DoctorDto>> GetApprovedDoctorsAsync()
        {
            var doctors = await _doctorRepository.GetApprovedDoctorsAsync();

            return doctors.Select(d => new DoctorDto
            {
                DoctorId = d.DoctorId,
                FullName = d.User.FullName,
                Specialization = d.Specialization.Name,
                LicenseNumber = d.LicenseNumber,
                ConsultationFee = d.ConsultationFee,
                ProfileImage = d.ProfileImage
            }).ToList();
        }

        public async Task<DoctorDto?> GetDoctorByIdAsync(int doctorId)
        {
            var doctor = await _doctorRepository.GetApprovedByIdAsync(doctorId);

            if (doctor == null)
                return null;

            return new DoctorDto
            {
                DoctorId = doctor.DoctorId,
                FullName = doctor.User.FullName,
                Specialization = doctor.Specialization.Name,
                LicenseNumber = doctor.LicenseNumber,
                ConsultationFee = doctor.ConsultationFee,
                ProfileImage = doctor.ProfileImage
            };
        }

        public async Task ApproveDoctorAsync(int doctorId)
        {
            var doctor = await GetPendingDoctorAsync(doctorId);

            doctor.ApprovalStatus = DoctorApprovalStatus.Approved;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RejectDoctorAsync(int doctorId)
        {
            var doctor = await GetPendingDoctorAsync(doctorId);

            doctor.ApprovalStatus = DoctorApprovalStatus.Rejected;

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<MediCare.Services.Entities.Doctor>
            GetPendingDoctorAsync(int doctorId)
        {
            var doctor = await _doctorRepository.GetByIdForUpdateAsync(doctorId);

            if (doctor == null)
                throw new NotFoundException("Doctor not found.");

            if (doctor.ApprovalStatus != DoctorApprovalStatus.Pending)
            {
                throw new ConflictException(
                    "Only pending doctors can be approved or rejected.");
            }

            return doctor;
        }
    }
}
