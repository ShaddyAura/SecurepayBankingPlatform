namespace Domain.Entities;

public class Customer
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Customer | Admin | Auditor
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool Is2FAEnabled { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

   
}
