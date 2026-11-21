using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Services.Entities
{
    [Table("DoctorSchedules")]
    public class DoctorSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public int SlotDurationMinutes { get; set; } = 30;

        public bool IsAvailable { get; set; } = true;
    }
}