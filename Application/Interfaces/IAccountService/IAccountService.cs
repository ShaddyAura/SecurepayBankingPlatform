using Application.DTOs.Account;

namespace Application.Interfaces.IAccountService;

public interface IAccountService
{
    Task<AccountDto?> GetMyAccountAsync(Guid customerId);
    Task<AccountDto>  CreateAccountAsync(CreateAccountDto dto);
}
