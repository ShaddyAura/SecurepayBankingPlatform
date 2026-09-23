using System.ComponentModel.DataAnnotations;

namespace DigiWeb.Models.Auth;

public class VerifyOtpModel
{
    public Guid CustomerId { get; set; }

    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
    public string Otp { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;
}
