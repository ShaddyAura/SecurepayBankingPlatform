using System.ComponentModel.DataAnnotations;

namespace DigiWeb.Models.Auth;

public class ChangePasswordModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Minimum 8 characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
