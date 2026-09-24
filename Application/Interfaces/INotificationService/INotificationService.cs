namespace Application.Interfaces.INotificationService;

public interface INotificationService
{
    Task PushBalanceUpdateAsync(string customerId, decimal newBalance);
    Task PushTransferStatusAsync(string customerId, Guid transactionId, string status, decimal amount);
}
