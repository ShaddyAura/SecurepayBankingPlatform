namespace Application.DTOs.Transfer;

public class TransferResultDto
{
    public Guid    TransactionId  { get; set; }
    public Guid    FromAccountId  { get; set; }
    public Guid    ToAccountId    { get; set; }
    public decimal Amount         { get; set; }
    public string  Status         { get; set; } = string.Empty;
    public string  IdempotencyKey { get; set; } = string.Empty;
    public string? Description    { get; set; }
    public string  ResultCode     { get; set; } = string.Empty; // SUCCESS | DUPLICATE
    public DateTime CreatedAt     { get; set; }
}
