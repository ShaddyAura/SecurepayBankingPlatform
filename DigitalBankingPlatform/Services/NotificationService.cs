using Application.Interfaces.INotificationService;
using DigitalBankingPlatform.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DigitalBankingPlatform.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<BankingHub> _hub;

    public NotificationService(IHubContext<BankingHub> hub)
    {
        _hub = hub;
    }

    // Push live balance to the customer's group
    public async Task PushBalanceUpdateAsync(string customerId, decimal newBalance)
    {
        await _hub.Clients
            .Group($"customer_{customerId}")
            .SendAsync("BalanceUpdated", newBalance);
    }

    // Push transfer status change to the customer's group
    public async Task PushTransferStatusAsync(string customerId, Guid transactionId, string status, decimal amount)
    {
        await _hub.Clients
            .Group($"customer_{customerId}")
            .SendAsync("TransferStatusChanged", new
            {
                TransactionId = transactionId,
                Status        = status,
                Amount        = amount,
                UpdatedAt     = DateTime.UtcNow
            });
    }
}
