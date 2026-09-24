using System.ComponentModel.DataAnnotations;

namespace DigiWeb.Models.Dashboard;

public class TransferModel
{
    [Required(ErrorMessage = "Destination account number is required")]
    public string ToAccountNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();
}
