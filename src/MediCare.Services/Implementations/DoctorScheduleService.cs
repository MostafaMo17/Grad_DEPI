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

        public async Task<List<DoctorScheduleDto>> GetMySchedulesAsync(int userId)
        {
            var doctor = await GetApprovedDoctorAsync(userId);

            var doctorSchedules = await _doctorScheduleRepository
                .GetByDoctorIdAsync(doctor.DoctorId);

            return doctorSchedules.Select(MapToDto).ToList();
        }

        public async Task<List<DoctorScheduleDto>> SaveMySchedulesAsync(int userId, SaveDoctorScheduleDto dto)
        {
            var doctor = await GetApprovedDoctorAsync(userId);

            if (dto == null)
                throw new BadRequestException("The schedule data is required.");

            var days = dto.Days ?? new List<DoctorScheduleDayDto>();

            // Each day can appear only once.
            if (days.Select(d => d.DayOfWeek).Distinct().Count() != days.Count)
                throw new BadRequestException("Each day of the week can appear only once.");

            var newSchedules = new List<DoctorSchedule>();

            foreach (var day in days)
            {
                if (day.EndTime <= day.StartTime)
                    throw new BadRequestException(
                        $"The end time must be after the start time ({day.DayOfWeek}).");

                if (day.SlotLengthMinutes < 5 || day.SlotLengthMinutes > 240)
                    throw new BadRequestException(
                        $"The slot length must be between 5 and 240 minutes ({day.DayOfWeek}).");

                var availableMinutes = (day.EndTime - day.StartTime).TotalMinutes;
                if (availableMinutes < day.SlotLengthMinutes)
                    throw new BadRequestException(
                        $"The working hours are shorter than one appointment slot ({day.DayOfWeek}).");

                newSchedules.Add(new DoctorSchedule
                {
                    DoctorId = doctor.DoctorId,
                    DayOfWeek = day.DayOfWeek,
                    StartTime = day.StartTime,
                    EndTime = day.EndTime,
                    SlotLengthMinutes = day.SlotLengthMinutes,
                    IsAvailable = true
                });
            }

            await _doctorScheduleRepository.ReplaceForDoctorAsync(doctor.DoctorId, newSchedules);

            var saved = await _doctorScheduleRepository.GetByDoctorIdAsync(doctor.DoctorId);
            return saved.Select(MapToDto).ToList();
        }

        // The logged-in user must have an approved doctor profile.
        private async Task<Doctor> GetApprovedDoctorAsync(int userId)
        {
            if (userId <= 0)
                throw new UnauthorizedException("A valid user account is required.");

            var doctor = await _doctorRepository.GetByUserIdAsync(userId);

            if (doctor == null)
                throw new ForbiddenException("A doctor profile is required.");

            if (doctor.ApprovalStatus != DoctorApprovalStatus.Approved)
                throw new ForbiddenException("The doctor account must be approved.");

            return doctor;
        }

        private static DoctorScheduleDto MapToDto(DoctorSchedule schedule)
        {
            return new DoctorScheduleDto
            {
                ScheduleId = schedule.ScheduleId,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                SlotLengthMinutes = schedule.SlotLengthMinutes,
                IsAvailable = schedule.IsAvailable
            };
        }
    }
}
