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
            Name = "Svíčková na smetaně",
            Description = "Tradiční hovězí pečeně s karlovarským knedlíkem a brusinkami.",
            Price = 189.00m,
            IsActive = true
        },
        new Meal
        {
            Id = Guid.NewGuid(),
            Name = "Smažený vepřový řízek",
            Description = "Smažený řízek z vepřové pečeně, podávaný s vídeňským bramborovým salátem.",
            Price = 165.00m,
            IsActive = true
        },
        new Meal
        {
            Id = Guid.NewGuid(),
            Name = "Špagety Carbonara",
            Description = "Krémové špagety s pancettou, žloutkem a parmazánem.",
            Price = 145.00m,
            IsActive = true
        },
        new Meal
        {
            Id = Guid.NewGuid(),
            Name = "Smažený sýr v bulce",
            Description = "Eidam v křupavé strouhance, tatarská omáčka a hranolky.",
            Price = 135.00m,
            IsActive = true
        },
        new Meal
        {
            Id = Guid.NewGuid(),
            Name = "Čočka na kyselo",
            Description = "Čočka s uzeným masem, sázeným vejcem a kyselou okurkou.",
            Price = 125.00m,
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
            AvailablePortions = 30
        },
        new MenuItem
        {
            Id = Guid.NewGuid(),
            Date = today,
            MealId = meals[1].Id,
            AvailablePortions = 45
        },
        new MenuItem
        {
            Id = Guid.NewGuid(),
            Date = today,
            MealId = meals[2].Id,
            AvailablePortions = 5
        },
        new MenuItem
        {
            Id = Guid.NewGuid(),
            Date = tomorrow,
            MealId = meals[3].Id,
            AvailablePortions = 50
        },
        new MenuItem
        {
            Id = Guid.NewGuid(),
            Date = tomorrow,
            MealId = meals[4].Id,
            AvailablePortions = 40
        }
    };

    db.MenuItems.AddRange(menuItems);
    await db.SaveChangesAsync();

    return TypedResults.Ok(new { message = "Database reset and seeded successfully" });
}).WithName("ResetAndSeed");

app.MapGet("/", () => "DbManager is running");

app.Run();
