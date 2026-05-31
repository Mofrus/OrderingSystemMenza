using UTB.Minute.Contracts.Menu;
using UTB.Minute.Db.Entities;

namespace UTB.Minute.WebApi.Mappers;

public static class MenuItemMapper
{
    public static MenuItemDto ToDto(this MenuItem menuItem)
    {
        return new MenuItemDto
        {
            Id = menuItem.Id,
            Date = menuItem.Date,
            MealId = menuItem.MealId,
            MealName = menuItem.Meal?.Name ?? string.Empty,
            AvailablePortions = menuItem.AvailablePortions
        };
    }
}