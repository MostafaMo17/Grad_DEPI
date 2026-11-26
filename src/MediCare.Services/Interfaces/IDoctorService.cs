
using MediCare.Services.Dtos;

namespace MediCare.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IReadOnlyList<DoctorDto>> GetApprovedDoctorsAsync();

        Task<DoctorDto?> GetDoctorByIdAsync(int doctorId);

        Task ApproveDoctorAsync(int doctorId);

        Task RejectDoctorAsync(int doctorId);
    }
}
