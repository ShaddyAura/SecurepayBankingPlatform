using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DigitalBankingPlatform.Hubs;

[Authorize]
public class BankingHub : Hub
{
    // Client joins their personal group using CustomerId so we can push to them specifically
    public async Task JoinAccountGroup(string customerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{customerId}");
    }

    public async Task LeaveAccountGroup(string customerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer_{customerId}");
    }
}
