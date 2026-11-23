using System;
using MediCare.Services.Entities;
using MediCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;  
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;




namespace MediCare.Data.Repositories
{
    public class DoctorScheduleRepository : IDoctorScheduleRepository
    {
        private readonly AppDbContext _dbContext;
        public DoctorScheduleRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<DoctorSchedule>> GetByDoctorIdAsync(int doctorId)
        {

            return await _dbContext.DoctorSchedules
                .AsNoTracking()
                .Where(ds => ds.DoctorId == doctorId)
                .OrderBy(ds => ds.DayOfWeek)
                .ThenBy(ds => ds.StartTime)
                .ToListAsync();
        }
    }
}
