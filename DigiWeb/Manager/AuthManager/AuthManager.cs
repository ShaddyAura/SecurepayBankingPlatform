using DigiWeb.Models.Auth;
using DigiWeb.Models.Dashboard;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DigiWeb.Manager.AuthManager;

public class AuthManager : IAuthManager
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/auth";

    public AuthManager(HttpClient http)
    {
        _http = http;
    }

    // Attaches Bearer token for protected endpoints
    private void Attach(string token)
        => _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public async Task<(bool Success, string Message, TokenModel? Token)> LoginAsync(LoginModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/login", model);
            var json     = await response.Content.ReadFromJsonAsync<ApiWrapper<TokenModel>>();
            return response.IsSuccessStatusCode && json?.Data != null
                ? (true,  json.Message ?? "Login successful.", json.Data)
                : (false, json?.Message ?? "Login failed.", null);
        }
        catch (Exception ex) { return (false, ex.Message, null); }
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/register", new
            {
                model.Name, model.Email, model.PhoneNumber, model.Password, model.Role
            });
            var json = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Registration successful.")
                : (false, json?.Message ?? "Registration failed.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/forgot-password", model);
            var json     = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "OTP sent.")
                : (false, json?.Message ?? "Failed.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/reset-password", new
            {
                model.CustomerId, model.Otp, NewPassword = model.NewPassword
            });
            var json = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Password reset.")
                : (false, json?.Message ?? "Failed.");
        }
        catch (Exception ex) { return (false, ex.Message); }
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
        catch (Exception ex) { return (false, ex.Message); }
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
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task LogoutAsync()
    {
        try { await _http.PostAsync($"{Base}/logout", null); }
        catch { }
    }

    // --- Methods that require the access token ---

    public async Task<(bool, string)> ChangePasswordAsync(ChangePasswordModel model, string accessToken)
    {
        try
        {
            Attach(accessToken);
            var response = await _http.PostAsJsonAsync($"{Base}/change-password", new
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword     = model.NewPassword
            });
            var json = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Password changed.")
                : (false, json?.Message ?? "Failed.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task Enable2FAAsync(string accessToken)
    {
        try { Attach(accessToken); await _http.PostAsync($"{Base}/enable-2fa", null); }
        catch { }
    }

    public async Task Disable2FAAsync(string accessToken)
    {
        try { Attach(accessToken); await _http.PostAsync($"{Base}/disable-2fa", null); }
        catch { }
    }

    public async Task RevokeSessionAsync(string refreshToken, string accessToken)
    {
        try
        {
            Attach(accessToken);
            await _http.PostAsJsonAsync($"{Base}/logout", refreshToken);
        }
        catch { }
    }

    public async Task LogoutAllAsync(string accessToken)
    {
        try { Attach(accessToken); await _http.PostAsync($"{Base}/logout-all", null); }
        catch { }
    }

    private class ApiWrapper<T>
    {
        public bool    Success { get; set; }
        public string? Message { get; set; }
        public T?      Data    { get; set; }
    }
}
