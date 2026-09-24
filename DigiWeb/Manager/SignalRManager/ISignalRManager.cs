namespace DigiWeb.Manager.SignalRManager;

public interface ISignalRManager : IAsyncDisposable
{
    bool IsConnected { get; }
    event Action<decimal>?             BalanceUpdated;
    event Action<TransferStatusEvent>? TransferStatusChanged;
    Task ConnectAsync(string accessToken, string customerId);
    Task DisconnectAsync();
}

public class TransferStatusEvent
{
    public Guid     TransactionId { get; set; }
    public string   Status        { get; set; } = string.Empty;
    public decimal  Amount        { get; set; }
    public DateTime UpdatedAt     { get; set; }
}
