using MediCare.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediCare.Data.Seeding
{
    public class SpecializationSeeder
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SpecializationSeeder> _logger;

        public SpecializationSeeder(AppDbContext dbContext, ILogger<SpecializationSeeder> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // If there is already at least one specialization, do nothing
            bool hasSpecializations = await _dbContext.Specializations.AnyAsync();
            if (hasSpecializations)
            {
                _logger.LogInformation("Specializations already exist. Skipping specialization seeding.");
                return;
            }

            var specializations = new List<Specialization>
        {
            new Specialization { Name = "Cardiology" },
            new Specialization { Name = "Pediatrics" },
            new Specialization { Name = "Dermatology" },
            new Specialization { Name = "General Medicine" },
            new Specialization { Name = "Orthopedics" },
            new Specialization { Name = "Neurology" },
            new Specialization { Name = "Ophthalmology" },
            new Specialization { Name = "Dentistry" }
        };

            _dbContext.Specializations.AddRange(specializations);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} specializations.", specializations.Count);
        }
    }
}
