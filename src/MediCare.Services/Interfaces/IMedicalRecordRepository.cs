
using MediCare.Services.Entities;

namespace MediCare.Services.Interfaces
{
    public interface IMedicalRecordRepository
    {
        Task<MedicalRecord?> GetByIdAsync(int medicalRecordId);

        Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId);

        Task<IReadOnlyList<MedicalRecord>> GetByPatientIdAsync(int patientId);

        Task<IReadOnlyList<MedicalRecord>> GetByPatientUserIdAsync(int patientUserId);

        Task<IReadOnlyList<MedicalRecord>> GetByDoctorUserIdAsync(int doctorUserId);

        Task AddAsync(MedicalRecord medicalRecord);
    }
}
