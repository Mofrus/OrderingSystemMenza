using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using UTB.Minute.Contracts.Orders;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;
using UTB.Minute.WebApi.Mappers;
using UTB.Minute.WebApi.Services;

namespace UTB.Minute.WebApi.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/orders");

        group.MapGet("/", GetAllOrders)
            .WithName("GetAllOrders"); 

        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder");

        group.MapPatch("/{id}/status", UpdateOrderStatus)
            .WithName("UpdateOrderStatus")
            .RequireAuthorization("Cook");

        group.MapGet("/debug-claims", (ClaimsPrincipal user) =>
        {
            return TypedResults.Ok(new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated,
                AuthenticationType = user.Identity?.AuthenticationType,
                Name = user.Identity?.Name,
                Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }).RequireAuthorization();
    }

    private static async Task<IResult> GetAllOrders(MinuteDbContext db, ClaimsPrincipal user, [FromQuery] string[]? ids)
    {
        var query = db.Orders
            .Include(o => o.MenuItem)
            .Where(o => o.Status != UTB.Minute.Db.Entities.OrderStatus.Completed);

        // If not Cook or Admin, only show orders that match provided IDs
        if (!user.IsInRole("Cook") && !user.IsInRole("Admin") && !user.IsInRole("cook") && !user.IsInRole("admin"))
        {
            if (ids == null || ids.Length == 0)
            {
                // Guest without specific IDs shouldn't see any orders
                return TypedResults.Ok(new List<OrderDto>());
            }
            
            var idGuids = ids.Select(i => Guid.TryParse(i, out var g) ? g : Guid.Empty).ToList();
            query = query.Where(o => idGuids.Contains(o.Id));
        }

        var orders = await query.ToListAsync();
        var orderDtos = orders.Select(o => o.ToDto()).ToList();
        return TypedResults.Ok(orderDtos);
    }

    private static async Task<IResult> CreateOrder(CreateOrderDto dto, MinuteDbContext db, NotificationService notificationService)
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
            StudentIdentifier = $"Guest-{Guid.NewGuid().ToString()[..6]}",
            Status = UTB.Minute.Db.Entities.OrderStatus.Preparing
        };

        menuItem.AvailablePortions--;

        db.Orders.Add(order);
        
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return TypedResults.Conflict("Při vytváření objednávky došlo ke konfliktu. Pravděpodobně byla prodána poslední porce. Zkuste to prosím znovu.");
        }

        var orderDto = order.ToDto();
        
        // Notify cooks
        await notificationService.BroadcastAsync($"{{\"type\":\"OrderCreated\",\"orderId\":\"{order.Id}\",\"menuItemId\":\"{order.MenuItemId}\"}}");

        return TypedResults.Created($"/orders/{order.Id}", orderDto);
    }

    private static async Task<IResult> UpdateOrderStatus(Guid id, UpdateOrderStatusDto dto, MinuteDbContext db, NotificationService notificationService)
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

        // Validation: Block invalid state transitions
        var currentStatus = order.Status;
        bool isValidTransition = (currentStatus, newStatus) switch
        {
            // Same state is usually a no-op but safe to allow from client retries
            var (curr, next) when curr == next => true,
            
            // Valid transitions
            (UTB.Minute.Db.Entities.OrderStatus.Preparing, UTB.Minute.Db.Entities.OrderStatus.Ready) => true,
            (UTB.Minute.Db.Entities.OrderStatus.Preparing, UTB.Minute.Db.Entities.OrderStatus.Cancelled) => true,
            (UTB.Minute.Db.Entities.OrderStatus.Ready, UTB.Minute.Db.Entities.OrderStatus.Completed) => true,
            (UTB.Minute.Db.Entities.OrderStatus.Ready, UTB.Minute.Db.Entities.OrderStatus.Cancelled) => true,
            
            // Any other transition is invalid
            _ => false
        };

        if (!isValidTransition)
        {
            return TypedResults.BadRequest($"Neplatný přechod stavu objednávky z '{currentStatus}' na '{newStatus}'.");
        }

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var orderDto = order.ToDto();
        
        // Notify student (and cooks)
        await notificationService.BroadcastAsync($"{{\"type\":\"OrderStatusUpdated\",\"orderId\":\"{order.Id}\",\"status\":\"{newStatus}\",\"student\":\"{order.StudentIdentifier}\"}}");

        return TypedResults.Ok(orderDto);
    }
}
