using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Services.Entities
{
    [Table("DoctorVacations")]
    public class DoctorVacation
    {
        [Key]
        public int VacationId { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; } = null!;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        [MaxLength(300)]
        public string? Reason { get; set; }
    }
}