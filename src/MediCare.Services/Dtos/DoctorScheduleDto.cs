using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Dtos
{
  public class DoctorScheduleDto
    {
        public int ScheduleId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SlotLengthMinutes { get; set; }
        public bool IsAvailable { get; set; }
    }
}
