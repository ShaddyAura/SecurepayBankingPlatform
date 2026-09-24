using DigiWeb.Components;
using DigiWeb.Manager.AuthManager;
using DigiWeb.Manager.DashboardManager;
using DigiWeb.Manager.SessionManager;
using DigiWeb.Manager.SignalRManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBase = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7290";

builder.Services.AddHttpClient<IAuthManager, AuthManager>(client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IDashboardManager, DashboardManager>(client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// SignalR manager — scoped so each user session gets their own connection
builder.Services.AddScoped<ISignalRManager, SignalRManager>();

// Session state — holds logged-in user token + profile per circuit
builder.Services.AddScoped<SessionManager>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
