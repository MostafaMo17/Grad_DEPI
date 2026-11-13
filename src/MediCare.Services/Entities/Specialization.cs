
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MediCare.Services.Entities
{
    [Table("Specializations")]
    public class Specialization
    {
        [Key]
        public int SpecializationId { get; set; }

        // Must be unique
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
