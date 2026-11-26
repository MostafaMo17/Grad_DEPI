
using MediCare.Services.Entities;

namespace MediCare.Services.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByUserIdAsync(int userId);

        Task<Doctor?> GetByIdAsync(int doctorId);

        Task<Doctor?> GetByIdForUpdateAsync(int doctorId);

        Task<Doctor?> GetApprovedByIdAsync(int doctorId);

        Task<IReadOnlyList<Doctor>> GetApprovedDoctorsAsync();
    }
}
