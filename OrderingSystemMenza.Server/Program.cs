using OrderingSystemMenza.Models; // Ujisti se, že máš správný namespace pro ModelsContext

var builder = WebApplication.CreateBuilder(args);

// 1. Aspire Service Defaults (standardní nastavení)
builder.AddServiceDefaults();

// 2. Registrace PostgreSQL pomocí Aspire komponenty
// "sqldb" musí odpovídat názvu, který jsi definoval v AppHostu přes .AddDatabase("sqldb")
builder.AddNpgsqlDbContext<ModelsContext>("sqldb");

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// 3. Automatická migrace (volitelné, ale užitečné pro vývoj)
// Toto zajistí, že se tabulky v Dockeru vytvoří samy při startu
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ModelsContext>();
    context.Database.EnsureCreated(); // Pro jednoduchost, nebo context.Database.Migrate();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Tady můžeš přidat endpointy, které pracují s DB
var api = app.MapGroup("/api");

api.MapGet("/orders", async (ModelsContext db) => 
{
    // Ukázka načtení dat z Postgresu
    return await db.Orders.ToListAsync(); 
});

app.MapDefaultEndpoints();
app.UseFileServer();
app.Run();

// Tvůj record WeatherForecast zůstává...