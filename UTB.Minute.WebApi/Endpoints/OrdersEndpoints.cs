using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts.Orders;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;
using UTB.Minute.WebApi.Mappers;

namespace UTB.Minute.WebApi.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        app.MapGet("/orders", GetAllOrders)
            .WithName("GetAllOrders");

        app.MapPost("/orders", CreateOrder)
            .WithName("CreateOrder");

        app.MapPatch("/orders/{id}/status", UpdateOrderStatus)
            .WithName("UpdateOrderStatus");
    }

    private static async Task<IResult> GetAllOrders(MinuteDbContext db)
    {
        var orders = await db.Orders
            .Include(o => o.MenuItem)
            .Where(o => o.Status != UTB.Minute.Db.Entities.OrderStatus.Completed)
            .ToListAsync();

        var orderDtos = orders.Select(o => o.ToDto()).ToList();
        return TypedResults.Ok(orderDtos);
    }

    private static async Task<IResult> CreateOrder(CreateOrderDto dto, MinuteDbContext db)
    {
        var menuItem = await db.MenuItems
            .Include(m => m.Meal)
            .FirstOrDefaultAsync(m => m.Id == dto.MenuItemId);

        if (menuItem == null)
        {
            return TypedResults.BadRequest($"MenuItem with id {dto.MenuItemId} not found");
        }

        if (menuItem.AvailablePortions <= 0)
        {
            return TypedResults.BadRequest("No available portions for this menu item");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            MenuItemId = dto.MenuItemId,
            StudentIdentifier = "student@utb.cz",
            Status = UTB.Minute.Db.Entities.OrderStatus.Preparing
        };

        menuItem.AvailablePortions--;

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var orderDto = order.ToDto();
        return TypedResults.Created($"/orders/{order.Id}", orderDto);
    }

    private static async Task<IResult> UpdateOrderStatus(Guid id, UpdateOrderStatusDto dto, MinuteDbContext db)
    {
        var order = await db.Orders
            .Include(o => o.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return TypedResults.NotFound($"Order with id {id} not found");
        }

        if (!Enum.IsDefined(dto.Status))
        {
            return TypedResults.BadRequest("Invalid order status.");
        }

        var newStatus = OrderMapper.ToDbStatus(dto.Status);

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var orderDto = order.ToDto();
        return TypedResults.Ok(orderDto);
    }
}
