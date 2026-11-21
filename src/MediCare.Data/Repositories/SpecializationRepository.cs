using MediCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories
{
    public class SpecializationRepository : ISpecializationRepository
    {
        private readonly AppDbContext _dbContext;

        public SpecializationRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsAsync(int specializationId)
        {
            return await _dbContext.Specializations.AnyAsync(s => s.SpecializationId == specializationId);
        }
    }
}
