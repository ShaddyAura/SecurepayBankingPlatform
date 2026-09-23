using DigiWeb.Models.Auth;
using System.Net.Http.Json;
using System.Text.Json;

namespace DigiWeb.Manager.AuthManager;

public class AuthManager : IAuthManager
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/auth";

    public AuthManager(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string Message, TokenModel? Token)> LoginAsync(LoginModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/login", model);
            var json     = await response.Content.ReadFromJsonAsync<ApiWrapper<TokenModel>>();

            if (response.IsSuccessStatusCode && json?.Data != null)
                return (true, json.Message ?? "Login successful.", json.Data);

            return (false, json?.Message ?? "Login failed.", null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/register", new
            {
                model.Name,
                model.Email,
                model.PhoneNumber,
                model.Password,
                model.Role
            });

            var json = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Registration successful.")
                : (false, json?.Message ?? "Registration failed.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/forgot-password", model);
            var json     = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "OTP sent to your email.")
                : (false, json?.Message ?? "Failed to send OTP.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/reset-password", new
            {
                model.CustomerId,
                model.Otp,
                NewPassword = model.NewPassword
            });
            var json = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Password reset successful.")
                : (false, json?.Message ?? "Failed to reset password.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> VerifyEmailOtpAsync(VerifyOtpModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/verify-email", model);
            var json     = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Email verified.")
                : (false, json?.Message ?? "Invalid OTP.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> Verify2FAOtpAsync(VerifyOtpModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/verify-2fa", model);
            var json     = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "2FA verified.")
                : (false, json?.Message ?? "Invalid OTP.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _http.PostAsync($"{Base}/logout", null);
        }
        catch { /* silent */ }
    }

    // Matches ApiResponse<T> from the API
    private class ApiWrapper<T>
    {
        public bool    Success { get; set; }
        public string? Message { get; set; }
        public T?      Data    { get; set; }
    }
}
