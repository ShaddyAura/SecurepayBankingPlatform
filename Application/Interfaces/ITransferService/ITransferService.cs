using Application.DTOs.Transfer;

namespace Application.Interfaces.ITransferService;

public interface ITransferService
{
    Task<TransferResultDto> TransferAsync(TransferRequestDto dto);
    Task<TransferResultDto?> GetTransferByIdAsync(Guid transactionId);
}
