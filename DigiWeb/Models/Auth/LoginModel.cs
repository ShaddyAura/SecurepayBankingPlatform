using System.ComponentModel.DataAnnotations;

namespace DigiWeb.Models.Auth;

public class LoginModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Minimum 6 characters")]
    public string Password { get; set; } = string.Empty;
}
