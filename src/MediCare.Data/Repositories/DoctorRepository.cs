
using MediCare.Services.Entities;
using MediCare.Services.Enums;
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

        public async Task<Doctor?> GetByIdAsync(int doctorId)
        {
            return await _dbContext.Doctors
                .AsNoTracking()
                .Include(d => d.Specialization)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        }

        public async Task<Doctor?> GetByIdForUpdateAsync(int doctorId)
        {
            return await _dbContext.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        }

        public async Task<Doctor?> GetApprovedByIdAsync(int doctorId)
        {
            return await _dbContext.Doctors
                .AsNoTracking()
                .Include(d => d.Specialization)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d =>
                    d.DoctorId == doctorId &&
                    d.ApprovalStatus == DoctorApprovalStatus.Approved);
        }

        public async Task<IReadOnlyList<Doctor>> GetApprovedDoctorsAsync()
        {
            return await _dbContext.Doctors
                .AsNoTracking()
                .Include(d => d.Specialization)
                .Include(d => d.User)
                .Where(d => d.ApprovalStatus == DoctorApprovalStatus.Approved)
                .ToListAsync();
        }
    }
}
