using ApiMessage;
using Application.DTOs.Transaction;
using Application.Interfaces.IAccountService;
using Application.Interfaces.ITransactionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalBankingPlatform.Controllers.Transaction;

[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _txService;
    private readonly IAccountService     _accountService;

    public TransactionController(ITransactionService txService, IAccountService accountService)
    {
        _txService      = txService;
        _accountService = accountService;
    }

    // GET api/v1/transactions?page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var customerId = GetCustomerId();
            var account    = await _accountService.GetMyAccountAsync(customerId)
                ?? throw new Exception("Account not found.");

            var history = await _txService.GetHistoryAsync(account.AccountId, page, pageSize);
            return Ok(ApiResponse<TransactionHistoryDto>.Ok(history));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<TransactionHistoryDto>.Fail(ex.Message)); }
    }

    // GET api/v1/transactions/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tx = await _txService.GetByIdAsync(id);
            return tx is null
                ? NotFound(ApiResponse<TransactionDto>.Fail("Transaction not found."))
                : Ok(ApiResponse<TransactionDto>.Ok(tx));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<TransactionDto>.Fail(ex.Message)); }
    }

    private Guid GetCustomerId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("Invalid token."));
}
