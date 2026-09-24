using ApiMessage;
using Application.DTOs.Transfer;
using Application.Interfaces.ITransferService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalBankingPlatform.Controllers.Transfer;

[ApiController]
[Route("api/v1/transfer")]
[Authorize(Policy = "CustomerOnly")]
public class TransferController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransferController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    // POST api/v1/transfer — initiate fund transfer
    [HttpPost]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
    {
        try
        {
            dto.FromAccountId = GetAccountId();
            var result        = await _transferService.TransferAsync(dto);
            return Ok(ApiResponse<TransferResultDto>.Ok(result, "Transfer processed."));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<TransferResultDto>.Fail(ex.Message)); }
    }

    // GET api/v1/transfer/{id} — get status of a specific transfer
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        try
        {
            var result = await _transferService.GetTransferByIdAsync(id);
            return result is null
                ? NotFound(ApiResponse<TransferResultDto>.Fail("Transaction not found."))
                : Ok(ApiResponse<TransferResultDto>.Ok(result));
        }
        catch (Exception ex) { return BadRequest(ApiResponse<TransferResultDto>.Fail(ex.Message)); }
    }

    private Guid GetAccountId() =>
        Guid.Parse(User.FindFirstValue("accountId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("Invalid token."));
}
