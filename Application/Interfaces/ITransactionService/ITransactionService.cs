using Application.DTOs.Transaction;

namespace Application.Interfaces.ITransactionService;

public interface ITransactionService
{
    Task<TransactionHistoryDto> GetHistoryAsync(Guid accountId, int page, int pageSize);
    Task<TransactionDto?> GetByIdAsync(Guid transactionId);
}
