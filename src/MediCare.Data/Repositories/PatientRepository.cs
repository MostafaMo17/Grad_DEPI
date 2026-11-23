using MediCare.Services.Entities;
using MediCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _dbContext;

        public PatientRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Patient?> GetByUserIdAsync(int userId)
        {
            return await _dbContext.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}
