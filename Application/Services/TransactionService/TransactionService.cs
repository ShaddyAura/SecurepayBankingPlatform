using Application.DTOs.Transaction;
using Application.Interfaces.ITransactionService;
using Dapper;
using Infrastructure.Interfaces;

namespace Application.Services.TransactionService;

public class TransactionService : ITransactionService
{
    private readonly IGenericRepository _repo;

    public TransactionService(IGenericRepository repo)
    {
        _repo = repo;
    }

    public async Task<TransactionHistoryDto> GetHistoryAsync(Guid accountId, int page, int pageSize)
    {
        var p = new DynamicParameters();
        p.Add("@AccountId",  accountId);
        p.Add("@PageNumber", page);
        p.Add("@PageSize",   pageSize);

        // sp_GetTransactionHistory returns two result sets: rows + total count
        return await _repo.QueryMultipleAsync<TransactionHistoryDto>(
            "sp_GetTransactionHistory",
            p,
            async multi =>
            {
                var rows  = (await multi.ReadAsync<TransactionDto>()).ToList();
                var total = await multi.ReadSingleAsync<int>();
                return new TransactionHistoryDto
                {
                    Transactions = rows,
                    TotalCount   = total,
                    PageNumber   = page,
                    PageSize     = pageSize
                };
            });
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid transactionId)
    {
        var p = new DynamicParameters();
        p.Add("@TransactionId", transactionId);
        return await _repo.QuerySingleOrDefaultAsync<TransactionDto>("sp_GetTransactionById", p);
    }
}
