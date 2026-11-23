using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Patient, Doctor or Admin
        public string Role { get; set; } = string.Empty;

        public int? PatientId { get; set; }

        public int? DoctorId { get; set; }

        // Only for doctors: Pending, Approved or Rejected. Null for everyone else
        public string? DoctorApprovalStatus { get; set; }
    }
}
