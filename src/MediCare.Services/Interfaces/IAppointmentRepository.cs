using MediCare.Services.Entities;

namespace MediCare.Services.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(int appointmentId);
    }
}