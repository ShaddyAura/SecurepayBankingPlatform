namespace Application.DTOs.Auth;

public class ResetPasswordDto
{
    public Guid CustomerId { get; set; }
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
