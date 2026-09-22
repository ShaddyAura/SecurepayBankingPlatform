namespace Domain.Entities;

public class Transaction
{
    public Guid TransactionId { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending"; 
    public string IdempotencyKey { get; set; } = string.Empty; 
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

}
