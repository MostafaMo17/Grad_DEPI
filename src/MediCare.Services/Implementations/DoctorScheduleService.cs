using System.Collections.Generic;
using System.Threading.Tasks;
using MediCare.Services.Dtos;
using MediCare.Services.Enums;
using MediCare.Services.Exceptions;
using MediCare.Services.Interfaces;
using MediCare.Services.Entities;
using System.Linq;

namespace MediCare.Services.Implementations
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IDoctorRepository _doctorRepository;

        public DoctorScheduleService(
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorRepository doctorRepository)
        {
            _doctorScheduleRepository = doctorScheduleRepository;
            _doctorRepository = doctorRepository;
        }

        public Task<List<DoctorScheduleDto>> GetDoctorScheduleAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DoctorScheduleDto>> GetMySchedulesAsync(int userId)
        {
            if (userId <= 0)
                throw new UnauthorizedException("A valid user account is required.");

            var doctor = await _doctorRepository.GetByUserIdAsync(userId);

            if (doctor == null)
                throw new ForbiddenException("A doctor profile is required.");

            if (doctor.ApprovalStatus != DoctorApprovalStatus.Approved)
                throw new ForbiddenException("The doctor account must be approved.");

            var doctorSchedules = await _doctorScheduleRepository
                .GetByDoctorIdAsync(doctor.DoctorId);

            var doctorScheduleDtos = new List<DoctorScheduleDto>();

            foreach (var schedule in doctorSchedules)
            {
                var dto = new DoctorScheduleDto
                {
                    ScheduleId = schedule.ScheduleId,
                    DayOfWeek = schedule.DayOfWeek,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    SlotLengthMinutes = schedule.SlotLengthMinutes,
                    IsAvailable = schedule.IsAvailable
                };

                doctorScheduleDtos.Add(dto);
            }

            return doctorScheduleDtos;
        }
    }
}