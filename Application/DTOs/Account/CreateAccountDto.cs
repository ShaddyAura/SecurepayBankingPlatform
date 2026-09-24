namespace Application.DTOs.Account;

public class CreateAccountDto
{
    public Guid   CustomerId     { get; set; }
    public string AccountType    { get; set; } = "Saving";
    public decimal InitialDeposit { get; set; } = 0;
}
