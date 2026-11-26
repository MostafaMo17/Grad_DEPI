using System;

namespace MediCare.Web.Models.Api;

// These classes only describe the JSON that the API sends and receives
// MediCare.Web does not reference the API projects, so we write them here again.

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterPatientRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }

    // "Male" or "Female"
    public string Gender { get; set; } = string.Empty;
}

public class RegisterDoctorRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int SpecializationId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
}

public class UserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? PatientId { get; set; }
    public int? DoctorId { get; set; }

    // Only for doctors: Pending, Approved or Rejected
    public string? DoctorApprovalStatus { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserDto User { get; set; } = new UserDto();
}