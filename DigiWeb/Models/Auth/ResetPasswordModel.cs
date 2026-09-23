using System.ComponentModel.DataAnnotations;

namespace DigiWeb.Models.Auth;

public class ResetPasswordModel
{
    public Guid CustomerId { get; set; }

    [Required(ErrorMessage = "OTP is required")]
    public string Otp { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Minimum 8 characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
