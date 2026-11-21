using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace MediCare.Services.Entities
{
    // The single account table for all roles (Patient, Doctor, Admin)
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
