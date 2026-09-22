using Application.DTOs.Auth;

namespace Application.Interfaces.IAuthservice;

public interface IAuthService
{
    Task<CustomerDto> RegisterAsync(RegisterDto dto);

    Task<TokenDto> LoginAsync(LoginDto dto);
    Task LogoutAsync(string refreshToken);
    Task LogoutAllDevicesAsync(Guid customerId);

    Task<TokenDto> RefreshTokenAsync(string refreshToken, string deviceInfo, string ipAddress);

    Task SendEmailOtpAsync(Guid customerId);
    Task<bool> VerifyEmailOtpAsync(VerifyOtpDto dto);

    Task SendPhoneOtpAsync(Guid customerId);
    Task<bool> VerifyPhoneOtpAsync(VerifyOtpDto dto);

    Task<bool> Enable2FAAsync(Guid customerId);
    Task<bool> Disable2FAAsync(Guid customerId);
    Task<bool> Verify2FAOtpAsync(VerifyOtpDto dto);

    Task ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task<bool> ChangePasswordAsync(ChangePasswordDto dto);

    Task<IEnumerable<RefreshTokenDto>> GetActiveSessionsAsync(Guid customerId);
}
