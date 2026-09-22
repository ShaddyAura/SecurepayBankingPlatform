namespace Application.Interfaces;

public class RefreshTokenDto
{
    public Guid TokenId { get; set; }
    public Guid CustomerId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool Revoked { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
