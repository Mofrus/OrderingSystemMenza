using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts.Menu;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;
using UTB.Minute.WebApi.Mappers;

namespace UTB.Minute.WebApi.Endpoints;

public static class MenuEndpoints
{
    public static void MapMenuEndpoints(this WebApplication app)
    {
        app.MapGet("/menu", GetAllMenuItems)
            .WithName("GetAllMenuItems")
            .WithOpenApi();

        app.MapGet("/menu/date/{date}", GetMenuItemsByDate)
            .WithName("GetMenuItemsByDate")
            .WithOpenApi();

        app.MapPost("/menu", CreateMenuItem)
            .WithName("CreateMenuItem")
            .WithOpenApi();

        app.MapPut("/menu/{id}", UpdateMenuItem)
            .WithName("UpdateMenuItem")
            .WithOpenApi();

        app.MapDelete("/menu/{id}", DeleteMenuItem)
            .WithName("DeleteMenuItem")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllMenuItems(MinuteDbContext db)
    {
        var menuItems = await db.MenuItems
            .Include(m => m.Meal)
            .ToListAsync();

        var menuItemDtos = menuItems.Select(m => m.ToDto()).ToList();
        return TypedResults.Ok(menuItemDtos);
    }

    private static async Task<IResult> GetMenuItemsByDate(DateOnly date, MinuteDbContext db)
    {
        var menuItems = await db.MenuItems
            .Where(m => m.Date == date)
            .Include(m => m.Meal)
            .ToListAsync();

        var menuItemDtos = menuItems.Select(m => m.ToDto()).ToList();
        return TypedResults.Ok(menuItemDtos);
    }

    private static async Task<IResult> CreateMenuItem(CreateMenuItemDto dto, MinuteDbContext db)
    {
        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == dto.MealId);
        if (meal == null)
        {
            return TypedResults.BadRequest($"Meal with id {dto.MealId} not found");
        }

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Date = dto.Date,
            MealId = dto.MealId,
            AvailablePortions = dto.AvailablePortions
        };

        db.MenuItems.Add(menuItem);
        await db.SaveChangesAsync();

        var menuItemDto = menuItem.ToDto();
        return TypedResults.Created($"/menu/{menuItem.Id}", menuItemDto);
    }

    private static async Task<IResult> UpdateMenuItem(Guid id, UpdateMenuItemDto dto, MinuteDbContext db)
    {
        var menuItem = await db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
        if (menuItem == null)
        {
            return TypedResults.NotFound($"MenuItem with id {id} not found");
        }

        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == dto.MealId);
        if (meal == null)
        {
            return TypedResults.BadRequest($"Meal with id {dto.MealId} not found");
        }

        menuItem.Date = dto.Date;
        menuItem.MealId = dto.MealId;
        menuItem.AvailablePortions = dto.AvailablePortions;
        menuItem.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var menuItemDto = menuItem.ToDto();
        return TypedResults.Ok(menuItemDto);
    }

    private static async Task<IResult> DeleteMenuItem(Guid id, MinuteDbContext db)
    {
        var menuItem = await db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
        if (menuItem == null)
        {
            return TypedResults.NotFound($"MenuItem with id {id} not found");
        }

        db.MenuItems.Remove(menuItem);
        await db.SaveChangesAsync();

        return TypedResults.Ok();
    }
}
