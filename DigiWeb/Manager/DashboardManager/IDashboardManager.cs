using DigiWeb.Models.Dashboard;

namespace DigiWeb.Manager.DashboardManager;

public interface IDashboardManager
{
    Task<AccountModel?>              GetMyAccountAsync();
    Task<(bool, string, AccountModel?)> CreateAccountAsync(CreateAccountModel model);
    Task<TransactionHistoryModel?>   GetTransactionsAsync(int page = 1, int pageSize = 10);
    Task<(bool, string)>             TransferAsync(TransferModel model, Guid fromAccountId);
    Task<ProfileModel?>              GetProfileAsync();
    Task<(bool, string)>             UpdateProfileAsync(UpdateProfileModel model);
    Task<IEnumerable<SessionModel>>  GetActiveSessionsAsync();
}
