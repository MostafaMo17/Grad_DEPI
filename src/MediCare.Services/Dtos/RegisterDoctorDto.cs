using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MediCare.Services.Dtos
{
    // to POST /api/v1/auth/register/doctor.
    // The doctor starts as Pending and cannot use doctor only features until an Admin approves
    public class RegisterDoctorDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string Password { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int SpecializationId { get; set; }

        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Range(0, 99999999.99)]
        public decimal ConsultationFee { get; set; }
    }
}
