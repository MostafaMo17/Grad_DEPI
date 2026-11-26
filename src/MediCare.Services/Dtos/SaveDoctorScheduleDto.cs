using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Dtos
{
    // Body of PUT /api/v1/doctor/schedules.
    // The doctor sends the days they work; days not in the list become days off.
    public class SaveDoctorScheduleDto
    {
        public List<DoctorScheduleDayDto> Days { get; set; } = new List<DoctorScheduleDayDto>();
    }

    public class DoctorScheduleDayDto
    {
        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int SlotLengthMinutes { get; set; } = 30;
    }
}
