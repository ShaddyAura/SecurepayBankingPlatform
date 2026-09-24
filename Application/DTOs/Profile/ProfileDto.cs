namespace Application.DTOs.Profile;

public class ProfileDto
{
    public Guid   CustomerId      { get; set; }
    public string Name            { get; set; } = string.Empty;
    public string Email           { get; set; } = string.Empty;
    public string? PhoneNumber    { get; set; }
    public string Role            { get; set; } = string.Empty;
    public bool   IsEmailVerified { get; set; }
    public bool   IsPhoneVerified { get; set; }
    public bool   Is2FAEnabled    { get; set; }
    public DateTime CreatedAt     { get; set; }
}
