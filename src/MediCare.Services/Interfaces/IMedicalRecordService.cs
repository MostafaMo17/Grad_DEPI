
using MediCare.Services.Dtos;

namespace MediCare.Services.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<MedicalRecordDto?> GetByIdAsync(
            int medicalRecordId,
            int currentUserId,
            string currentRole);

        Task<MedicalRecordDto?> GetByAppointmentIdAsync(
            int appointmentId,
            int currentUserId,
            string currentRole);

        Task<IReadOnlyList<MedicalRecordDto>> GetMyRecordsAsync(
            int currentUserId,
            string currentRole);

        Task<MedicalRecordDto> CreateAsync(
            int appointmentId,
            int doctorUserId,
            CreateMedicalRecordDto dto);
    }
}
