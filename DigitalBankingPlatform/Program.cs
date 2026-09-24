using Application.Interfaces.IAccountService;
using Application.Interfaces.IAuthservice;
using Application.Interfaces.INotificationService;
using Application.Interfaces.IProfileService;
using Application.Interfaces.ITransactionService;
using Application.Interfaces.ITransferService;
using Application.Mappings.AuthMapping;
using Application.Mappings.DashboardMapping;
using Application.Services.AccountService;
using Application.Services.AuthService;
using Application.Services.ProfileService;
using Application.Services.TransactionService;
using Application.Services.TransferService;
using DigitalBankingPlatform.Hubs;
using DigitalBankingPlatform.Services;
using Infrastructure.Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS — allow Blazor frontend origins
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // required for SignalR
});

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IGenericRepository, GenericRepository>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AuthMappingProfile>();
    cfg.AddProfile<DashboardMappingProfile>();
});

builder.Services.AddScoped<IAuthService,        AuthService>();
builder.Services.AddScoped<IAccountService,     AccountService>();
builder.Services.AddScoped<ITransferService,    TransferService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IProfileService,     ProfileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// SignalR
builder.Services.AddSignalR();

// JWT
var jwtKey      = builder.Configuration["Jwt:Key"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtAudience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew                = TimeSpan.Zero // no grace period on expiry
    };
});

// Role-based policies
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("CustomerOnly",   p => p.RequireRole("Customer"));
    opt.AddPolicy("AdminOnly",      p => p.RequireRole("Admin"));
    opt.AddPolicy("AuditorOnly",    p => p.RequireRole("Auditor"));
    opt.AddPolicy("AdminOrAuditor", p => p.RequireRole("Admin", "Auditor"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT bearer
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "SecurePay API", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste your JWT token here"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "SecurePay API v1");
        opt.RoutePrefix   = string.Empty; // Swagger at root
        opt.DocumentTitle = "SecurePay API";
    });
}

app.UseHttpsRedirection();
app.UseCors("BlazorPolicy"); 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<BankingHub>("/hubs/banking"); // SignalR endpoint

app.Run();
