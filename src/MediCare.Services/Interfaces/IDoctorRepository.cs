using MediCare.Services.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface IDoctorRepository
    {
        // Returns the doctor profile that belongs to this user account, or null if there is none.
        Task<Doctor?> GetByUserIdAsync(int userId);
    }
}
