using ApiMessage;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.IAuthservice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalBankingPlatform.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(ApiResponse<CustomerDto>.Ok(result, "Registration successful."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<CustomerDto>.Fail(ex.Message)); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        dto.IpAddress  = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        dto.DeviceInfo = Request.Headers.UserAgent.ToString();
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<TokenDto>.Ok(result, "Login successful."));
        }
        catch (Exception ex) when (ex.Message == "2FA_REQUIRED")
        {
            return Accepted(ApiResponse<string>.Ok("2FA_REQUIRED", "OTP sent to registered device."));
        }
        catch (Exception ex) { return Unauthorized(ApiResponse<TokenDto>.Fail(ex.Message)); }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(
                refreshToken,
                Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return Ok(ApiResponse<TokenDto>.Ok(result, "Token refreshed."));
        }
        catch (Exception ex) { return Unauthorized(ApiResponse<TokenDto>.Fail(ex.Message)); }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        try
        {
            await _authService.LogoutAsync(refreshToken);
            return Ok(ApiResponse<string>.Ok("Logged out.", "Logged out successfully."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        try
        {
            await _authService.LogoutAllDevicesAsync(GetCustomerIdFromToken());
            return Ok(ApiResponse<string>.Ok("All sessions revoked.", "Logged out from all devices."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
    }

    [HttpPost("send-email-otp")]
    [Authorize]
    public async Task<IActionResult> SendEmailOtp()
    {
        try
        {
            await _authService.SendEmailOtpAsync(GetCustomerIdFromToken());
            return Ok(ApiResponse<string>.Ok("OTP sent.", "Email OTP sent."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
    }

    [HttpPost("verify-email")]
    [Authorize]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpDto dto)
    {
        try
        {
            dto.CustomerId = GetCustomerIdFromToken();
            dto.Purpose    = "EmailVerification";
            var result     = await _authService.VerifyEmailOtpAsync(dto);
            return result
                ? Ok(ApiResponse<bool>.Ok(true, "Email verified."))
                : BadRequest(ApiResponse<bool>.Fail("Invalid or expired OTP."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<bool>.Fail(ex.Message)); }
    }

    [HttpPost("send-phone-otp")]
    [Authorize]
    public async Task<IActionResult> SendPhoneOtp()
    {
        try
        {
            await _authService.SendPhoneOtpAsync(GetCustomerIdFromToken());
            return Ok(ApiResponse<string>.Ok("OTP sent.", "Phone OTP sent."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
    }

    [HttpPost("verify-phone")]
    [Authorize]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyOtpDto dto)
    {
        try
        {
            dto.CustomerId = GetCustomerIdFromToken();
            dto.Purpose    = "PhoneVerification";
            var result     = await _authService.VerifyPhoneOtpAsync(dto);
            return result
                ? Ok(ApiResponse<bool>.Ok(true, "Phone verified."))
                : BadRequest(ApiResponse<bool>.Fail("Invalid or expired OTP."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<bool>.Fail(ex.Message)); }
    }

    [HttpPost("enable-2fa")]
    [Authorize]
    public async Task<IActionResult> Enable2FA()
    {
        try
        {
            await _authService.Enable2FAAsync(GetCustomerIdFromToken());
            return Ok(ApiResponse<bool>.Ok(true, "2FA enabled."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<bool>.Fail(ex.Message)); }
    }

    [HttpPost("disable-2fa")]
    [Authorize]
    public async Task<IActionResult> Disable2FA()
    {
        try
        {
            await _authService.Disable2FAAsync(GetCustomerIdFromToken());
            return Ok(ApiResponse<bool>.Ok(true, "2FA disabled."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<bool>.Fail(ex.Message)); }
    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> Verify2FA([FromBody] VerifyOtpDto dto)
    {
        try
        {
            dto.Purpose = "TwoFactor";
            var result  = await _authService.Verify2FAOtpAsync(dto);
            return result
                ? Ok(ApiResponse<bool>.Ok(true, "2FA verified."))
                : BadRequest(ApiResponse<bool>.Fail("Invalid or expired OTP."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<bool>.Fail(ex.Message)); }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            await _authService.ForgotPasswordAsync(dto);
            return Ok(ApiResponse<string>.Ok("OTP sent.", "Password reset OTP sent to email."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(dto);
            return result
                ? Ok(ApiResponse<bool>.Ok(true, "Password reset successful."))
                : BadRequest(ApiResponse<bool>.Fail("Invalid or expired OTP."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<bool>.Fail(ex.Message)); }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            dto.CustomerId = GetCustomerIdFromToken();
            var result     = await _authService.ChangePasswordAsync(dto);
            return result
                ? Ok(ApiResponse<bool>.Ok(true, "Password changed."))
                : BadRequest(ApiResponse<bool>.Fail("Failed to change password."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<bool>.Fail(ex.Message)); }
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetSessions()
    {
        try
        {
            var sessions = await _authService.GetActiveSessionsAsync(GetCustomerIdFromToken());
            return Ok(ApiResponse<IEnumerable<RefreshTokenDto>>.Ok(sessions, "Active sessions."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<IEnumerable<RefreshTokenDto>>.Fail(ex.Message)); }
    }

    private Guid GetCustomerIdFromToken()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new Exception("Invalid token.");
        return Guid.Parse(sub);
    }
}
