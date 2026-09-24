using Microsoft.AspNetCore.SignalR.Client;

namespace DigiWeb.Manager.SignalRManager;

public class SignalRManager : ISignalRManager
{
    private HubConnection?  _connection;
    private readonly string _apiBase;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public event Action<decimal>?             BalanceUpdated;
    public event Action<TransferStatusEvent>? TransferStatusChanged;

    public SignalRManager(IConfiguration config)
    {
        _apiBase = config["ApiSettings:BaseUrl"] ?? "https://localhost:7290";
    }

    public async Task ConnectAsync(string accessToken, string customerId)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_apiBase}/hubs/banking", options =>
            {
                // attach JWT so the hub [Authorize] passes
                options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
            })
            .WithAutomaticReconnect()
            .Build();

        // Server pushes live balance after every transfer
        _connection.On<decimal>("BalanceUpdated", balance =>
            BalanceUpdated?.Invoke(balance));

        // Server pushes transfer status changes (Pending → Completed / Failed)
        _connection.On<TransferStatusEvent>("TransferStatusChanged", evt =>
            TransferStatusChanged?.Invoke(evt));

        await _connection.StartAsync();

        // Join personal group so server can target only this customer
        await _connection.InvokeAsync("JoinAccountGroup", customerId);
    }

    public async Task DisconnectAsync()
    {
        if (_connection is not null)
            await _connection.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
