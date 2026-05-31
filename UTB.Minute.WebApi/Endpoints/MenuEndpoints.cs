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
        var group = app.MapGroup("/menu-items").RequireAuthorization();

        group.MapGet("/", GetAllMenuItems)
            .WithName("GetAllMenuItems")
            .AllowAnonymous();

        group.MapPost("/", CreateMenuItem)
            .WithName("CreateMenuItem")
            .RequireAuthorization("Admin");

        group.MapPut("/{id}", UpdateMenuItem)
            .WithName("UpdateMenuItem")
            .RequireAuthorization("Admin");

        group.MapDelete("/{id}", DeleteMenuItem)
            .WithName("DeleteMenuItem")
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> GetAllMenuItems(MinuteDbContext db)
    {
        var menuItems = await db.MenuItems
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

        if (dto.AvailablePortions < 0)
        {
            return TypedResults.BadRequest("AvailablePortions must be non-negative.");
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
        return TypedResults.Created($"/menu-items/{menuItem.Id}", menuItemDto);
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

        return TypedResults.NoContent();
    }
}
