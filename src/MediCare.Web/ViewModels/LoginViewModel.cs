using System.ComponentModel.DataAnnotations;

namespace MediCare.Web.ViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;

    // The page the user wanted before being sent to the login page
    public string? ReturnUrl { get; set; }

    // Shows "your session ended" on the page
    public bool SessionExpired { get; set; }
}