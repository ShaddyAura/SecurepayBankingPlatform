using ApiMessage;
using Application.DTOs.Account;
using Application.Interfaces.IAccountService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalBankingPlatform.Controllers.Account;

[ApiController]
[Route("api/v1/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // GET api/v1/account/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyAccount()
    {
        try
        {
            var customerId = GetCustomerId();
            var account    = await _accountService.GetMyAccountAsync(customerId);
            return account is null
                ? NotFound(ApiResponse<AccountDto>.Fail("Account not found."))
                : Ok(ApiResponse<AccountDto>.Ok(account));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<AccountDto>.Fail(ex.Message)); }
    }

    // POST api/v1/account/create
    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        try
        {
            dto.CustomerId = GetCustomerId(); // always use token's customerId
            var account    = await _accountService.CreateAccountAsync(dto);
            return Ok(ApiResponse<AccountDto>.Ok(account, "Account created successfully."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<AccountDto>.Fail(ex.Message)); }
    }

    private Guid GetCustomerId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("Invalid token."));
}
