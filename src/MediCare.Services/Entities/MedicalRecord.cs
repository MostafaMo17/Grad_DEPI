using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Services.Entities
{
    [Table("MedicalRecords")]
    public class MedicalRecord
    {
        [Key]
        public int MedicalRecordId { get; set; }

        public int AppointmentId { get; set; }
        [ForeignKey(nameof(AppointmentId))]
        public Appointment Appointment { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Symptoms { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Diagnosis { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? TreatmentNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Prescription? Prescription { get; set; }
    }
}