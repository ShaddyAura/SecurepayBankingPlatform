namespace Application.DTOs.Account;

public class AccountDto
{
    public Guid    AccountId     { get; set; }
    public string  AccountNumber { get; set; } = string.Empty;
    public decimal Balance       { get; set; }
    public string  Status        { get; set; } = string.Empty;
    public string  AccountType   { get; set; } = string.Empty;
    public string  CustomerName  { get; set; } = string.Empty;
    public string  CustomerEmail { get; set; } = string.Empty;
    public DateTime CreatedAt    { get; set; }
}
