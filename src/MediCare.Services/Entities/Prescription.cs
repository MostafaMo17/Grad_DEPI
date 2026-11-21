using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Services.Entities
{
    [Table("Prescriptions")]
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        public int MedicalRecordId { get; set; }
        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord MedicalRecord { get; set; } = null!;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}