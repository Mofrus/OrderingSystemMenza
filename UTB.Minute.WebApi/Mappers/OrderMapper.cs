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
            Status = ToContractStatus(order.Status)
        };
    }

    private static Contracts.Enums.OrderStatus ToContractStatus(Db.Entities.OrderStatus status)
    {
        return status switch
        {
            Db.Entities.OrderStatus.Preparing => Contracts.Enums.OrderStatus.Preparing,
            Db.Entities.OrderStatus.Ready => Contracts.Enums.OrderStatus.Ready,
            Db.Entities.OrderStatus.Cancelled => Contracts.Enums.OrderStatus.Cancelled,
            Db.Entities.OrderStatus.Completed => Contracts.Enums.OrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static Db.Entities.OrderStatus ToDbStatus(Contracts.Enums.OrderStatus status)
    {
        return status switch
        {
            Contracts.Enums.OrderStatus.Preparing => Db.Entities.OrderStatus.Preparing,
            Contracts.Enums.OrderStatus.Ready => Db.Entities.OrderStatus.Ready,
            Contracts.Enums.OrderStatus.Cancelled => Db.Entities.OrderStatus.Cancelled,
            Contracts.Enums.OrderStatus.Completed => Db.Entities.OrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}