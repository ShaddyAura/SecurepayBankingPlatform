namespace DigiWeb.Models.Dashboard;

public class TransactionModel
{
    public Guid TransactionId { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Status  { get; set; } = string.Empty;
    public string Direction  { get; set; } = string.Empty; // Debit | Credit
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TransactionHistoryModel
{
    public List<TransactionModel> Transactions { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize  { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
