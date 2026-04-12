using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;
using UTB.Minute.WebApi.Mappers;

namespace UTB.Minute.WebApi.Endpoints;

public static class MealsEndpoints
{
    public static void MapMealsEndpoints(this WebApplication app)
    {
        app.MapGet("/meals", GetAllMeals)
            .WithName("GetAllMeals")
            .WithOpenApi();

        app.MapPost("/meals", CreateMeal)
            .WithName("CreateMeal")
            .WithOpenApi();

        app.MapPut("/meals/{id}", UpdateMeal)
            .WithName("UpdateMeal")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllMeals(MinuteDbContext db)
    {
        var meals = await db.Meals.ToListAsync();
        var mealDtos = meals.Select(m => m.ToDto()).ToList();
        return TypedResults.Ok(mealDtos);
    }

    private static async Task<IResult> CreateMeal(CreateMealDto dto, MinuteDbContext db)
    {
        var meal = new Meal
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsActive = true
        };

        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        var mealDto = meal.ToDto();
        return TypedResults.Created($"/meals/{meal.Id}", mealDto);
    }

    private static async Task<IResult> UpdateMeal(Guid id, UpdateMealDto dto, MinuteDbContext db)
    {
        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == id);
        if (meal == null)
        {
            return TypedResults.NotFound($"Meal with id {id} not found");
        }

        meal.Name = dto.Name;
        meal.Description = dto.Description;
        meal.Price = dto.Price;
        meal.IsActive = dto.IsActive;
        meal.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var mealDto = meal.ToDto();
        return TypedResults.Ok(mealDto);
    }
}
