using System;
using System.ComponentModel.DataAnnotations;

namespace MediCare.Web.ViewModels;

// The rules here match the API, so most mistakes are caught before the request is sent
// The API checks everything again (it has the final word)
public class RegisterPatientViewModel
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

    // The API requires: at least 6 characters, an uppercase letter, a lowercase letter, a digit and a symbol.
    [Required]
    [DataType(DataType.Password)]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    // "Male" or "Female"
    [Required]
    public string Gender { get; set; } = string.Empty;
}