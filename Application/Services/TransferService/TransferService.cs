using Application.DTOs.Transfer;
using Application.Interfaces.INotificationService;
using Application.Interfaces.ITransferService;
using Dapper;
using Infrastructure.Interfaces;

namespace Application.Services.TransferService;

public class TransferService : ITransferService
{
    private readonly IGenericRepository    _repo;
    private readonly INotificationService  _notify;

    public TransferService(IGenericRepository repo, INotificationService notify)
    {
        _repo   = repo;
        _notify = notify;
    }

    public async Task<TransferResultDto> TransferAsync(TransferRequestDto dto)
    {
        var lookupParams = new DynamicParameters();
        lookupParams.Add("@AccountNumber", dto.ToAccountNumber);
        var toAccount = await _repo.QuerySingleOrDefaultAsync<dynamic>("sp_GetAccountByNumber", lookupParams)
            ?? throw new Exception("Destination account not found.");

        var p = new DynamicParameters();
        p.Add("@FromAccountId",  dto.FromAccountId);
        p.Add("@ToAccountId",    (Guid)toAccount.AccountId);
        p.Add("@Amount",         dto.Amount);
        p.Add("@IdempotencyKey", dto.IdempotencyKey);
        p.Add("@Description",    dto.Description);

        var result = await _repo.QuerySingleOrDefaultAsync<TransferResultDto>("sp_TransferFunds", p)
            ?? throw new Exception("Transfer failed.");

        // Push real-time notifications to both sender and receiver
        await _notify.PushTransferStatusAsync(
            dto.FromAccountId.ToString(), result.TransactionId, result.Status, result.Amount);

        // Refresh sender balance and push update
        var balParams = new DynamicParameters();
        balParams.Add("@AccountId", dto.FromAccountId);
        var updatedAccount = await _repo.QuerySingleOrDefaultAsync<dynamic>("sp_GetAccountBalance", balParams);
        if (updatedAccount is not null)
            await _notify.PushBalanceUpdateAsync(dto.FromAccountId.ToString(), (decimal)updatedAccount.Balance);

        return result;
    }

    public async Task<TransferResultDto?> GetTransferByIdAsync(Guid transactionId)
    {
        var p = new DynamicParameters();
        p.Add("@TransactionId", transactionId);
        return await _repo.QuerySingleOrDefaultAsync<TransferResultDto>("sp_GetTransactionById", p);
    }
}
