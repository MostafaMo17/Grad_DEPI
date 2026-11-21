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

        // 1 for the first prescription of a record, then 2, 3, ...
        public int VersionNumber { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}