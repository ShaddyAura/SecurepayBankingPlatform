namespace Application.DTOs.Auth;

public class VerifyOtpDto
{
    public Guid CustomerId { get; set; }
    public string Otp { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // EmailVerification | PhoneVerification | TwoFactor | PasswordReset
}
