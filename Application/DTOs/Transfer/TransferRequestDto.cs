namespace Application.DTOs.Transfer;

public class TransferRequestDto
{
    public Guid    FromAccountId  { get; set; }
    public string  ToAccountNumber { get; set; } = string.Empty; // user types account number
    public decimal Amount          { get; set; }
    public string? Description     { get; set; }
    public string  IdempotencyKey  { get; set; } = Guid.NewGuid().ToString(); // client-generated
}
