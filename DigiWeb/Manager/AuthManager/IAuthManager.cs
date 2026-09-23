using DigiWeb.Models.Auth;

namespace DigiWeb.Manager.AuthManager;

public interface IAuthManager
{
    Task<(bool Success, string Message, TokenModel? Token)> LoginAsync(LoginModel model);
    Task<(bool Success, string Message)> RegisterAsync(RegisterModel model);
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordModel model);
    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordModel model);
    Task<(bool Success, string Message)> VerifyEmailOtpAsync(VerifyOtpModel model);
    Task<(bool Success, string Message)> Verify2FAOtpAsync(VerifyOtpModel model);
    Task LogoutAsync();
}
