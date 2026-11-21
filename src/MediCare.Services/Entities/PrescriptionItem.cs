using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Services.Entities
{
    [Table("PrescriptionItems")]
    public class PrescriptionItem
    {
        [Key]
        public int PrescriptionItemId { get; set; }

        public int PrescriptionId { get; set; }
        [ForeignKey(nameof(PrescriptionId))]
        public Prescription Prescription { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string MedicationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Duration { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Instructions { get; set; }
    }
}