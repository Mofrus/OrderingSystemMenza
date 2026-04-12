using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;
using UTB.Minute.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MinuteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

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

app.MapGet("/", () => "API is running");

app.MapMealsEndpoints();
app.MapMenuEndpoints();
app.MapOrdersEndpoints();

app.Run();