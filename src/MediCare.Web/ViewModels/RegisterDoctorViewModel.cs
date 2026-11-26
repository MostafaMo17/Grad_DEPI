using System.ComponentModel.DataAnnotations;

namespace MediCare.Web.ViewModels;

public class RegisterDoctorViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a specialization.")]
    [Display(Name = "Specialization")]
    public int SpecializationId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "License number")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 99999999.99)]
    [Display(Name = "Consultation fee")]
    public decimal ConsultationFee { get; set; }
}