using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using UTB.Minute.Db;
using UTB.Minute.WebApi.Endpoints;
using UTB.Minute.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<MinuteDbContext>("minute-db");

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
        // For local development with Aspire, Keycloak connection string is provided automatically
        var keycloakUrl = builder.Configuration.GetConnectionString("keycloak");
        if (!string.IsNullOrEmpty(keycloakUrl))
        {
            options.Authority = $"{keycloakUrl}/realms/menza";
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                NameClaimType = "preferred_username"
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.Principal?.Identity is ClaimsIdentity identity)
                    {
                        var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                        if (realmAccess != null)
                        {
                            using var json = System.Text.Json.JsonDocument.Parse(realmAccess);
                            if (json.RootElement.TryGetProperty("roles", out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                                }
                            }
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        }
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowAll");
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