using DigiWeb.Models.Auth;

namespace DigiWeb.Manager.SessionManager;

// Scoped — one instance per Blazor circuit (per user session)
public class SessionManager
{
    public TokenModel? CurrentUser { get; private set; }
    public bool        IsLoggedIn  => CurrentUser is not null;

    public event Action? OnChange;

    public void SetUser(TokenModel token)
    {
        CurrentUser = token;
        OnChange?.Invoke();
    }

    public void Clear()
    {
        CurrentUser = null;
        OnChange?.Invoke();
    }
}
