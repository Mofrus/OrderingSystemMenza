using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using UTB.Minute.Db;
using UTB.Minute.WebApi.Endpoints;
using UTB.Minute.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Try to get connection string from Aspire, fallback to local postgres if not found
var connectionString = builder.Configuration.GetConnectionString("minute-db") 
                      ?? "Host=localhost;Database=minute-db;Username=postgres;Password=postgres";

builder.AddNpgsqlDbContext<MinuteDbContext>("minute-db", configureSettings: settings => {
    settings.ConnectionString = connectionString;
});

builder.Services.AddSingleton<NotificationService>();

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Aspire provides Keycloak URL via service discovery, not connection strings
        var keycloakUrl = builder.Configuration["services:keycloak:http:0"]
                          ?? builder.Configuration.GetConnectionString("keycloak")
                          ?? "http://localhost:8080";
        keycloakUrl = keycloakUrl.TrimEnd('/');
        
        options.Authority = $"{keycloakUrl}/realms/menza";
        Console.WriteLine($"[JWT] Keycloak Authority configured: {options.Authority}");
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true, // Force validation so our custom validator is called
            IssuerValidator = (issuer, securityToken, validationParameters) =>
            {
                // Always accept the token's issuer
                return issuer;
            },
            NameClaimType = "preferred_username",
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity
                    && context.SecurityToken is Microsoft.IdentityModel.JsonWebTokens.JsonWebToken jwt)
                {
                    // Safely parse the realm_access JSON object directly from the JWT
                    if (jwt.TryGetPayloadValue<System.Text.Json.JsonElement>("realm_access", out var realmAccess)
                        && realmAccess.TryGetProperty("roles", out var roles)
                        && roles.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleStr = role.GetString();
                            if (!string.IsNullOrEmpty(roleStr))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleStr));
                                identity.AddClaim(new Claim("role", roleStr)); // Add both just to be safe
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
                logger.LogWarning("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

builder.Services.AddAuthorization(options =>
{
    // Combine multiple authentication schemes for default policy
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "TestScheme")
        .Build();

    options.AddPolicy("Admin", policy => policy
        .RequireRole("Admin")
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "TestScheme"));
    
    options.AddPolicy("Cook", policy => policy
        .RequireRole("Cook")
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "TestScheme"));
    
    options.AddPolicy("Student", policy => policy
        .RequireRole("Student")
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "TestScheme"));
});

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapGet("/", () => "API is running");

app.MapMealsEndpoints();
app.MapMenuEndpoints();
app.MapOrdersEndpoints();
app.MapNotificationsEndpoints();

app.Run();

public partial class Program { }