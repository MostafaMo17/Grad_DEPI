using MediCare.Services.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface IDoctorScheduleRepository
    {
            Task<List<DoctorSchedule>> GetByDoctorIdAsync(int doctorId);
        }
    }

