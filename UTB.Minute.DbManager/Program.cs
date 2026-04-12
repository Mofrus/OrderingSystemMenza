using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<MinuteDbContext>("minute-db");

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapPost("/db/reset-seed", async (MinuteDbContext db) =>
{
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    var meals = new List<Meal>
    {
        new Meal
        {
            Id = Guid.NewGuid(),
            Name = "Guláš",
            Description = "Tradiční český guláš",
            Price = 150.00m,
            IsActive = true
        },
        new Meal
        {
            Id = Guid.NewGuid(),
            Name = "Svíčková",
            Description = "Svíčková na smetaně",
            Price = 180.00m,
            IsActive = true
        },
        new Meal
        {
            Id = Guid.NewGuid(),
            Name = "Řízek",
            Description = "Smažený řízek",
            Price = 160.00m,
            IsActive = true
        }
    };

    db.Meals.AddRange(meals);
    await db.SaveChangesAsync();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var tomorrow = today.AddDays(1);

    var menuItems = new List<MenuItem>
    {
        new MenuItem
        {
            Id = Guid.NewGuid(),
            Date = today,
            MealId = meals[0].Id,
            AvailablePortions = 50
        },
        new MenuItem
        {
            Id = Guid.NewGuid(),
            Date = today,
            MealId = meals[1].Id,
            AvailablePortions = 40
        },
        new MenuItem
        {
            Id = Guid.NewGuid(),
            Date = tomorrow,
            MealId = meals[2].Id,
            AvailablePortions = 60
        }
    };

    db.MenuItems.AddRange(menuItems);
    await db.SaveChangesAsync();

    return TypedResults.Ok(new { message = "Database reset and seeded successfully" });
}).WithName("ResetAndSeed");

app.MapGet("/", () => "DbManager is running");

app.Run();
