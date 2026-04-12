using UTB.Minute.Contracts.Enums;

namespace UTB.Minute.Contracts.Orders;

public class OrderDto
{
    public Guid Id { get; set; }

    public Guid MenuItemId { get; set; }

    public string StudentIdentifier { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public OrderStatus Status { get; set; }
}
