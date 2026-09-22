using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.IAuthservice;
using AutoMapper;
using Dapper;
using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IGenericRepository _repo;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public AuthService(IGenericRepository repo, IMapper mapper, IConfiguration config)
    {
        _repo   = repo;
        _mapper = mapper;
        _config = config;
    }

    public async Task<CustomerDto> RegisterAsync(RegisterDto dto)
    {
        var customer = _mapper.Map<Customer>(dto);
        customer.PasswordHash = HashPassword(dto.Password);

        var p = new DynamicParameters();
        p.Add("@CustomerId",   customer.CustomerId);
        p.Add("@Name",         customer.Name);
        p.Add("@Email",        customer.Email);
        p.Add("@PhoneNumber",  customer.PhoneNumber);
        p.Add("@PasswordHash", customer.PasswordHash);
        p.Add("@Role",         customer.Role);

        var result = await _repo.QuerySingleOrDefaultAsync<Customer>("sp_CreateCustomer", p)
            ?? throw new Exception("Registration failed.");

        return _mapper.Map<CustomerDto>(result);
    }

    public async Task<TokenDto> LoginAsync(LoginDto dto)
    {
        var p = new DynamicParameters();
        p.Add("@Email", dto.Email);

        var customer = await _repo.QuerySingleOrDefaultAsync<Customer>("sp_GetCustomerByEmail", p)
            ?? throw new Exception("Invalid email or password.");

        if (!VerifyPassword(dto.Password, customer.PasswordHash))
            throw new Exception("Invalid email or password.");

        if (!customer.IsActive)
            throw new Exception("Account is inactive.");

        if (customer.Is2FAEnabled)
        {
            await Send2FAOtpInternalAsync(customer.CustomerId);
            throw new Exception("2FA_REQUIRED");
        }

        return await IssueTokenPairAsync(customer, dto.DeviceInfo ?? "Unknown", dto.IpAddress ?? "Unknown");
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var p = new DynamicParameters();
        p.Add("@Token", refreshToken);
        await _repo.ExecuteAsync("sp_RevokeRefreshToken", p);
    }

    public async Task LogoutAllDevicesAsync(Guid customerId)
    {
        var p = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        await _repo.ExecuteAsync("sp_RevokeAllSessions", p);
    }

    public async Task<TokenDto> RefreshTokenAsync(string refreshToken, string deviceInfo, string ipAddress)
    {
        var p = new DynamicParameters();
        p.Add("@Token", refreshToken);

        var token = await _repo.QuerySingleOrDefaultAsync<RefreshToken>("sp_GetRefreshToken", p)
            ?? throw new Exception("Invalid refresh token.");

        if (token.Revoked || token.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Refresh token is expired or revoked.");

        await LogoutAsync(refreshToken);

        var cp = new DynamicParameters();
        cp.Add("@CustomerId", token.CustomerId);

        var customer = await _repo.QuerySingleOrDefaultAsync<Customer>("sp_GetCustomerById", cp)
            ?? throw new Exception("Customer not found.");

        return await IssueTokenPairAsync(customer, deviceInfo, ipAddress);
    }

    public async Task SendEmailOtpAsync(Guid customerId)
    {
        var otp = GenerateOtp();
        var p   = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        p.Add("@Otp",        otp);
        p.Add("@Purpose",    "EmailVerification");
        p.Add("@ExpiresAt",  DateTime.UtcNow.AddMinutes(10));
        await _repo.ExecuteAsync("sp_SaveOtp", p);
        Console.WriteLine($"[EMAIL OTP] CustomerId={customerId} OTP={otp}");
    }

    public async Task<bool> VerifyEmailOtpAsync(VerifyOtpDto dto)
    {
        if (!await ValidateOtpAsync(dto.CustomerId, dto.Otp, "EmailVerification")) return false;
        var p = new DynamicParameters();
        p.Add("@CustomerId", dto.CustomerId);
        await _repo.ExecuteAsync("sp_SetEmailVerified", p);
        return true;
    }

    public async Task SendPhoneOtpAsync(Guid customerId)
    {
        var otp = GenerateOtp();
        var p   = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        p.Add("@Otp",        otp);
        p.Add("@Purpose",    "PhoneVerification");
        p.Add("@ExpiresAt",  DateTime.UtcNow.AddMinutes(10));
        await _repo.ExecuteAsync("sp_SaveOtp", p);
        Console.WriteLine($"[SMS OTP] CustomerId={customerId} OTP={otp}");
    }

    public async Task<bool> VerifyPhoneOtpAsync(VerifyOtpDto dto)
    {
        if (!await ValidateOtpAsync(dto.CustomerId, dto.Otp, "PhoneVerification")) return false;
        var p = new DynamicParameters();
        p.Add("@CustomerId", dto.CustomerId);
        await _repo.ExecuteAsync("sp_SetPhoneVerified", p);
        return true;
    }

    public async Task<bool> Enable2FAAsync(Guid customerId)
    {
        var p = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        p.Add("@Enable",     true);
        await _repo.ExecuteAsync("sp_Toggle2FA", p);
        return true;
    }

    public async Task<bool> Disable2FAAsync(Guid customerId)
    {
        var p = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        p.Add("@Enable",     false);
        await _repo.ExecuteAsync("sp_Toggle2FA", p);
        return true;
    }

    public async Task<bool> Verify2FAOtpAsync(VerifyOtpDto dto)
        => await ValidateOtpAsync(dto.CustomerId, dto.Otp, "TwoFactor");

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var ep = new DynamicParameters();
        ep.Add("@Email", dto.Email);

        var customer = await _repo.QuerySingleOrDefaultAsync<Customer>("sp_GetCustomerByEmail", ep)
            ?? throw new Exception("Email not found.");

        var otp = GenerateOtp();
        var p   = new DynamicParameters();
        p.Add("@CustomerId", customer.CustomerId);
        p.Add("@Otp",        otp);
        p.Add("@Purpose",    "PasswordReset");
        p.Add("@ExpiresAt",  DateTime.UtcNow.AddMinutes(15));
        await _repo.ExecuteAsync("sp_SaveOtp", p);
        Console.WriteLine($"[PASSWORD RESET OTP] CustomerId={customer.CustomerId} OTP={otp}");
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (!await ValidateOtpAsync(dto.CustomerId, dto.Otp, "PasswordReset")) return false;
        var p = new DynamicParameters();
        p.Add("@CustomerId",      dto.CustomerId);
        p.Add("@NewPasswordHash", HashPassword(dto.NewPassword));
        await _repo.ExecuteAsync("sp_UpdatePassword", p);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var cp = new DynamicParameters();
        cp.Add("@CustomerId", dto.CustomerId);

        var customer = await _repo.QuerySingleOrDefaultAsync<Customer>("sp_GetCustomerById", cp)
            ?? throw new Exception("Customer not found.");

        if (!VerifyPassword(dto.CurrentPassword, customer.PasswordHash))
            throw new Exception("Current password is incorrect.");

        var p = new DynamicParameters();
        p.Add("@CustomerId",      dto.CustomerId);
        p.Add("@NewPasswordHash", HashPassword(dto.NewPassword));
        await _repo.ExecuteAsync("sp_UpdatePassword", p);
        return true;
    }

    public async Task<IEnumerable<RefreshTokenDto>> GetActiveSessionsAsync(Guid customerId)
    {
        var p = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        var tokens = await _repo.QueryAsync<RefreshToken>("sp_GetActiveSessions", p);
        return _mapper.Map<IEnumerable<RefreshTokenDto>>(tokens);
    }

    // ---------------------------------------------------------------
    // Private Helpers
    // ---------------------------------------------------------------
    private async Task<TokenDto> IssueTokenPairAsync(Customer customer, string deviceInfo, string ipAddress)
    {
        var accessToken   = GenerateAccessToken(customer);
        var refreshToken  = GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7"));

        var p = new DynamicParameters();
        p.Add("@TokenId",    Guid.NewGuid());
        p.Add("@CustomerId", customer.CustomerId);
        p.Add("@Token",      refreshToken);
        p.Add("@DeviceInfo", deviceInfo);
        p.Add("@IpAddress",  ipAddress);
        p.Add("@ExpiresAt",  refreshExpiry);
        await _repo.ExecuteAsync("sp_SaveRefreshToken", p);

        return new TokenDto
        {
            AccessToken       = accessToken,
            RefreshToken      = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "15")),
            Role       = customer.Role,
            CustomerId = customer.CustomerId,
            Name       = customer.Name,
            Email      = customer.Email
        };
    }

    private string GenerateAccessToken(Customer customer)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "15"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   customer.CustomerId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email),
            new Claim(ClaimTypes.NameIdentifier,     customer.CustomerId.ToString()),
            new Claim(ClaimTypes.Role,               customer.Role),
            new Claim("name",                        customer.Name),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateOtp()
    {
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        return (Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000 + 100000).ToString();
    }

    private static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    private static bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);

    private async Task Send2FAOtpInternalAsync(Guid customerId)
    {
        var otp = GenerateOtp();
        var p   = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        p.Add("@Otp",        otp);
        p.Add("@Purpose",    "TwoFactor");
        p.Add("@ExpiresAt",  DateTime.UtcNow.AddMinutes(5));
        await _repo.ExecuteAsync("sp_SaveOtp", p);
        Console.WriteLine($"[2FA OTP] CustomerId={customerId} OTP={otp}");
    }

    private async Task<bool> ValidateOtpAsync(Guid customerId, string otp, string purpose)
    {
        var p = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        p.Add("@Otp",        otp);
        p.Add("@Purpose",    purpose);

        var result = await _repo.QuerySingleOrDefaultAsync<OtpResult>("sp_ValidateOtp", p);
        return result?.IsValid ?? false;
    }

    private sealed class OtpResult
    {
        public bool   IsValid { get; set; }
        public string Reason  { get; set; } = string.Empty;
    }
}
