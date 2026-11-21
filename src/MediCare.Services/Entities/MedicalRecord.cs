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

        [Required]
        [MaxLength(1000)]
        public string TreatmentNotes { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Prescription versions of this record (an issued prescription is never edited, a new version is created)
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}