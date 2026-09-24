using Application.DTOs.Account;
using Application.Interfaces.IAccountService;
using AutoMapper;
using Dapper;
using Domain.Entities;
using Infrastructure.Interfaces;

namespace Application.Services.AccountService;

public class AccountService : IAccountService
{
    private readonly IGenericRepository _repo;
    private readonly IMapper _mapper;

    public AccountService(IGenericRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<AccountDto?> GetMyAccountAsync(Guid customerId)
    {
        var p = new DynamicParameters();
        p.Add("@CustomerId", customerId);
        return await _repo.QuerySingleOrDefaultAsync<AccountDto>("sp_GetAccountByCustomerId", p);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto)
    {
        // Generate unique 12-digit account number: type prefix + random digits
        var prefix = dto.AccountType switch
        {
            "Saving"  => "SAV",
            "Salary"  => "SAL",
            "Current" => "CUR",
            "Fixed"   => "FXD",
            _         => "ACC"
        };
        var random        = new Random();
        var accountNumber = $"{prefix}{DateTime.UtcNow:yyMMdd}{random.Next(1000, 9999)}";
        var accountId     = Guid.NewGuid();

        var p = new DynamicParameters();
        p.Add("@AccountId",      accountId);
        p.Add("@AccountNumber",  accountNumber);
        p.Add("@CustomerId",     dto.CustomerId);
        p.Add("@AccountType",    dto.AccountType);
        p.Add("@InitialDeposit", dto.InitialDeposit);

        var result = await _repo.QuerySingleOrDefaultAsync<AccountDto>("sp_CreateAccount", p)
            ?? throw new Exception("Account creation failed.");
        return result;
    }
}
