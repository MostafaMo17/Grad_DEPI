using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediCare.Services.Dtos;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface IDoctorScheduleService
    {
        Task<List<DoctorScheduleDto>> GetMySchedulesAsync(int userId);

    }
}
