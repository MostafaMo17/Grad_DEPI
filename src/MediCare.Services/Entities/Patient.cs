
using MediCare.Services.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MediCare.Services.Entities
{
    [Table("Patients")]
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        // (one profile per account)
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }
        public Gender Gender { get; set; }

        // Maintained by the patient after registration 
        public string? MedicalHistory { get; set; }
        public string? Allergies { get; set; }
    }
}
