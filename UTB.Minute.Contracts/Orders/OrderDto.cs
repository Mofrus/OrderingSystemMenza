using UTB.Minute.Contracts.Enums;
using UTB.Minute.Contracts.Menu;

namespace UTB.Minute.Contracts.Orders;

public class OrderDto
{
    public Guid Id { get; set; }

    public Guid MenuItemId { get; set; }
    
    public MenuItemDto MenuItem { get; set; } = null!;

    public string StudentIdentifier { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public OrderStatus Status { get; set; }
}
