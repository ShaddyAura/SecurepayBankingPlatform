using DigiWeb.Manager.SessionManager;
using DigiWeb.Models.Dashboard;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DigiWeb.Manager.DashboardManager;

public class DashboardManager : IDashboardManager
{
    private readonly HttpClient _http;
    private readonly SessionManager.SessionManager _session;

    public DashboardManager(HttpClient http, SessionManager.SessionManager session)
    {
        _http    = http;
        _session = session;
    }

    // Attaches JWT Bearer token from session before every call
    private void AttachToken()
    {
        if (_session.CurrentUser is not null)
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _session.CurrentUser.AccessToken);
    }

    public async Task<(bool, string, AccountModel?)> CreateAccountAsync(CreateAccountModel model)
    {
        try
        {
            AttachToken();
            var response = await _http.PostAsJsonAsync("api/v1/account/create", new
            {
                model.AccountType,
                model.InitialDeposit
            });
            var json = await response.Content.ReadFromJsonAsync<ApiWrapper<AccountModel>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Account created.", json?.Data)
                : (false, json?.Message ?? "Failed.", null);
        }
        catch (Exception ex) { return (false, ex.Message, null); }
    }

    public async Task<AccountModel?> GetMyAccountAsync()
    {
        try
        {
            AttachToken();
            var resp = await _http.GetFromJsonAsync<ApiWrapper<AccountModel>>("api/v1/account/my");
            return resp?.Data;
        }
        catch { return null; }
    }

    public async Task<TransactionHistoryModel?> GetTransactionsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            AttachToken();
            var resp = await _http.GetFromJsonAsync<ApiWrapper<TransactionHistoryModel>>(
                $"api/v1/transactions?page={page}&pageSize={pageSize}");
            return resp?.Data;
        }
        catch { return null; }
    }

    public async Task<(bool, string)> TransferAsync(TransferModel model, Guid fromAccountId)
    {
        try
        {
            AttachToken();
            var payload = new
            {
                FromAccountId   = fromAccountId,
                model.ToAccountNumber,
                model.Amount,
                model.Description,
                model.IdempotencyKey
            };
            var response = await _http.PostAsJsonAsync("api/v1/transfer", payload);
            var json     = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Transfer successful.")
                : (false, json?.Message ?? "Transfer failed.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<ProfileModel?> GetProfileAsync()
    {
        try
        {
            AttachToken();
            var resp = await _http.GetFromJsonAsync<ApiWrapper<ProfileModel>>("api/v1/profile");
            return resp?.Data;
        }
        catch { return null; }
    }

    public async Task<(bool, string)> UpdateProfileAsync(UpdateProfileModel model)
    {
        try
        {
            AttachToken();
            var response = await _http.PutAsJsonAsync("api/v1/profile", model);
            var json     = await response.Content.ReadFromJsonAsync<ApiWrapper<object>>();
            return response.IsSuccessStatusCode
                ? (true,  json?.Message ?? "Profile updated.")
                : (false, json?.Message ?? "Update failed.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<IEnumerable<SessionModel>> GetActiveSessionsAsync()
    {
        try
        {
            AttachToken();
            var resp = await _http.GetFromJsonAsync<ApiWrapper<IEnumerable<SessionModel>>>("api/v1/auth/sessions");
            return resp?.Data ?? Enumerable.Empty<SessionModel>();
        }
        catch { return Enumerable.Empty<SessionModel>(); }
    }

    private class ApiWrapper<T>
    {
        public bool    Success { get; set; }
        public string? Message { get; set; }
        public T?      Data    { get; set; }
    }
}
