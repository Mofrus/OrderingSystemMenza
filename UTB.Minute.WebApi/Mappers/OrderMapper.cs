using UTB.Minute.Contracts.Orders;
using UTB.Minute.Db.Entities;

namespace UTB.Minute.WebApi.Mappers;

public static class OrderMapper
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            MenuItemId = order.MenuItemId,
            StudentIdentifier = order.StudentIdentifier,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Status = ConvertStatus(order.Status)
        };
    }

    private static OrderStatus ConvertStatus(UTB.Minute.Db.Entities.OrderStatus status)
    {
        return status switch
        {
            UTB.Minute.Db.Entities.OrderStatus.Preparing => OrderStatus.Preparing,
            UTB.Minute.Db.Entities.OrderStatus.Ready => OrderStatus.Ready,
            UTB.Minute.Db.Entities.OrderStatus.Cancelled => OrderStatus.Cancelled,
            UTB.Minute.Db.Entities.OrderStatus.Completed => OrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static UTB.Minute.Db.Entities.OrderStatus ToDbStatus(OrderStatus status)
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