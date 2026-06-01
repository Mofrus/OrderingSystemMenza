using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<MinuteDbContext>("minute-db");

var app = builder.Build();

app.MapDefaultEndpoints();

// Auto-seed on startup: create schema and seed only if the DB is empty
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MinuteDbContext>();
    await db.Database.EnsureCreatedAsync();

    if (!await db.Meals.AnyAsync())
    {
        await SeedAsync(db);
        app.Logger.LogInformation("Database was empty — seeded successfully.");
    }
    else
    {
        app.Logger.LogInformation("Database already contains data — skipping seed.");
    }
}

// Manual reset+seed endpoint (still available for dev convenience)
app.MapPost("/db/reset-seed", async (MinuteDbContext db) =>
{
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
    await SeedAsync(db);
    return TypedResults.Ok(new { message = "Database reset and seeded successfully" });
}).WithName("ResetAndSeed");

app.MapGet("/", () => "DbManager is running");

app.Run();

static async Task SeedAsync(MinuteDbContext db)
{
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
    var menuItems = new List<MenuItem>();
    var rand = new Random(42);

    for (int i = 0; i < 7; i++)
    {
        var currentDate = today.AddDays(i);

        // Pick 2-3 random meals for each day
        var shuffledMeals = meals.OrderBy(x => rand.Next()).Take(rand.Next(2, 4)).ToList();

        foreach (var meal in shuffledMeals)
        {
            menuItems.Add(new MenuItem
            {
                Id = Guid.NewGuid(),
                Date = currentDate,
                MealId = meal.Id,
                AvailablePortions = rand.Next(5, 50)
            });
        }
    }

    db.MenuItems.AddRange(menuItems);
    await db.SaveChangesAsync();
}
