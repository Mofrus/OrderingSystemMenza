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
            .WithName("GetAllOrders")
            .WithOpenApi();

        app.MapGet("/orders/pending", GetPendingOrders)
            .WithName("GetPendingOrders")
            .WithOpenApi();

        app.MapGet("/orders/{id}", GetOrderById)
            .WithName("GetOrderById")
            .WithOpenApi();

        app.MapPost("/orders", CreateOrder)
            .WithName("CreateOrder")
            .WithOpenApi();

        app.MapPut("/orders/{id}/status", UpdateOrderStatus)
            .WithName("UpdateOrderStatus")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllOrders(MinuteDbContext db)
    {
        var orders = await db.Orders
            .Include(o => o.MenuItem)
            .ToListAsync();

        var orderDtos = orders.Select(o => o.ToDto()).ToList();
        return TypedResults.Ok(orderDtos);
    }

    private static async Task<IResult> GetPendingOrders(MinuteDbContext db)
    {
        var orders = await db.Orders
            .Include(o => o.MenuItem)
            .Where(o => o.Status != UTB.Minute.Db.Entities.OrderStatus.Completed)
            .ToListAsync();

        var orderDtos = orders.Select(o => o.ToDto()).ToList();
        return TypedResults.Ok(orderDtos);
    }

    private static async Task<IResult> GetOrderById(Guid id, MinuteDbContext db)
    {
        var order = await db.Orders
            .Include(o => o.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return TypedResults.NotFound($"Order with id {id} not found");
        }

        var orderDto = order.ToDto();
        return TypedResults.Ok(orderDto);
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

        var newStatus = OrderMapper.ToDbStatus(dto.Status);

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var orderDto = order.ToDto();
        return TypedResults.Ok(orderDto);
    }

    private static UTB.Minute.Db.Entities.OrderStatus ConvertToDbStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Preparing => UTB.Minute.Db.Entities.OrderStatus.Preparing,
            OrderStatus.Ready => UTB.Minute.Db.Entities.OrderStatus.Ready,
            OrderStatus.Cancelled => UTB.Minute.Db.Entities.OrderStatus.Cancelled,
            OrderStatus.Completed => UTB.Minute.Db.Entities.OrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
