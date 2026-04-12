using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MinuteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.MapPost("/reset-db", ResetDatabase)
    .WithName("ResetDatabase")
    .WithOpenApi();

app.MapGet("/", () => "DbManager is running");

app.Run();

static async Task<IResult> ResetDatabase(MinuteDbContext db)
{
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    await SeedTestData(db);

    return TypedResults.Ok(new { message = "Database reset and seeded successfully" });
}

static async Task SeedTestData(MinuteDbContext db)
{
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

    foreach (var meal in meals)
    {
        if (!db.Meals.Any(m => m.Name == meal.Name))
        {
            db.Meals.Add(meal);
        }
    }

    await db.SaveChangesAsync();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var tomorrow = today.AddDays(1);
    var nextWeek = today.AddDays(7);

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

    foreach (var item in menuItems)
    {
        if (!db.MenuItems.Any(m => m.Date == item.Date && m.MealId == item.MealId))
        {
            db.MenuItems.Add(item);
        }
    }

    await db.SaveChangesAsync();
}
