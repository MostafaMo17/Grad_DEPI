using MediCare.Services.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Services.Entities
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; } = null!;

        public int PatientId { get; set; }
        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        public DateOnly AppointmentDate { get; set; }
        public DateTime StartDateTimeUtc { get; set; }

        public DateTime EndDateTimeUtc { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        
        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Changes on every update. EF uses it to detect that two people changed the same row at the same time
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public MedicalRecord? MedicalRecord { get; set; }
    }
}