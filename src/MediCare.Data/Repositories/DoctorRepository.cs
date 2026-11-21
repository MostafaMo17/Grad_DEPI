using MediCare.Services.Entities;
using MediCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace MediCare.Data.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _dbContext;

        public DoctorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Doctor?> GetByUserIdAsync(int userId)
        {
            return await _dbContext.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }
    }
}
