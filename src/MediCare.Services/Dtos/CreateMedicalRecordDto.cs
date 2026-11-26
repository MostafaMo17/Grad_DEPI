
using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.Dtos
{
    public class CreateMedicalRecordDto
    {
        [Required]
        [MaxLength(1000)]
        public string Symptoms { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string TreatmentNotes { get; set; } = string.Empty;
    }
}
