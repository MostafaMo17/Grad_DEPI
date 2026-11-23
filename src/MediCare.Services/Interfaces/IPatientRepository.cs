using MediCare.Services.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface IPatientRepository
    {
        // Return the patient profile if it to this user account or null 
        Task<Patient?> GetByUserIdAsync(int userId);
    }
}
