using UTB.Minute.Db;
using UTB.Minute.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<MinuteDbContext>("minute-db");

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowAll");
}

app.MapDefaultEndpoints();

app.MapGet("/", () => "API is running");

app.MapMealsEndpoints();
app.MapMenuEndpoints();
app.MapOrdersEndpoints();

app.Run();

public partial class Program { }