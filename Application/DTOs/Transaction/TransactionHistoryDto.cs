namespace Application.DTOs.Transaction;

public class TransactionHistoryDto
{
    public IEnumerable<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    public int TotalCount  { get; set; }
    public int PageNumber  { get; set; }
    public int PageSize    { get; set; }
    public int TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);
}
