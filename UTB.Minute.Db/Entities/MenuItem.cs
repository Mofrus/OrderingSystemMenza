using System.ComponentModel.DataAnnotations;

namespace UTB.Minute.Db.Entities;

public class MenuItem
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public Guid MealId { get; set; }
    public Meal Meal { get; set; } = null!;

    [ConcurrencyCheck]
    public int AvailablePortions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}