using MediCare.Services.Enums;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MediCare.Services.Entities
{
    [Table("Doctors")]
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        // (one profile per account)
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public int SpecializationId { get; set; }
        [ForeignKey(nameof(SpecializationId))]
        public Specialization Specialization { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ConsultationFee { get; set; }

        // storage key or relative reference
        [MaxLength(500)]
        public string? ProfileImage { get; set; }

        // Where the doctor works (for example "Main Clinic - Room 402"), Used by the doctor search filter.
        [MaxLength(200)]
        public string? Location { get; set; }

        // New doctor registrations start as Pending and are not bookable until Admin approval.
        public DoctorApprovalStatus ApprovalStatus { get; set; } = DoctorApprovalStatus.Pending;

        public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
        public ICollection<DoctorVacation> DoctorVacations { get; set; } = new List<DoctorVacation>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
