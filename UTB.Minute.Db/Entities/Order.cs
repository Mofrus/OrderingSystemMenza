namespace UTB.Minute.Db.Entities;

public class Order
{
    public Guid Id { get; set; }

    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public string StudentIdentifier { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Preparing;

}
