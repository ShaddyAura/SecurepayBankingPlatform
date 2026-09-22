namespace Domain.Entities;

public class Account
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty; 
    public Guid CustomerId { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Active"; 
    public DateTime CreatedAt { get; set; }
    public Customer? Customer { get; set; }
}
