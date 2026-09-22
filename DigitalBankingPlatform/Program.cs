using Application.Interfaces.IAuthservice;
using Application.Mappings.AuthMapping;
using Application.Services.AuthService;
using Infrastructure.Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// Database — Dapper Context
// ---------------------------------------------------------------
builder.Services.AddSingleton<DapperContext>();

// ---------------------------------------------------------------
// Generic Repository
// ---------------------------------------------------------------
builder.Services.AddScoped<IGenericRepository, GenericRepository>();

// ---------------------------------------------------------------
// AutoMapper
// ---------------------------------------------------------------
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AuthMappingProfile>());

// ---------------------------------------------------------------
// Services
// ---------------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();

// ---------------------------------------------------------------
// JWT Authentication
// ---------------------------------------------------------------
var jwtKey      = builder.Configuration["Jwt:Key"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtAudience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew                = TimeSpan.Zero
    };
});

// ---------------------------------------------------------------
// Authorization — Role-Based Policies
// ---------------------------------------------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerOnly",   policy => policy.RequireRole("Customer"));
    options.AddPolicy("AdminOnly",      policy => policy.RequireRole("Admin"));
    options.AddPolicy("AuditorOnly",    policy => policy.RequireRole("Auditor"));
    options.AddPolicy("AdminOrAuditor", policy => policy.RequireRole("Admin", "Auditor"));
});

// ---------------------------------------------------------------
// Controllers
// ---------------------------------------------------------------
builder.Services.AddControllers();

// ---------------------------------------------------------------
// Swagger / OpenAPI with JWT Bearer support
// ---------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "SecurePay — Digital Banking API",
        Version     = "v1",
        Description = "Real-Time Fund Transfer & Digital Banking Platform",
        Contact     = new OpenApiContact
        {
            Name  = "SecurePay Team",
            Email = "dev@securepay.com"
        }
    });

    // Add JWT Bearer auth to Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token below. Example: eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------------------------------------------------------------
// Middleware Pipeline
// ---------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SecurePay API v1");
        options.RoutePrefix = string.Empty; // Swagger opens at root: http://localhost:PORT/
        options.DocumentTitle = "SecurePay API";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
