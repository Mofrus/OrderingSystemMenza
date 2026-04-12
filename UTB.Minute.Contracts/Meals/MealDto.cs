namespace UTB.Minute.Contracts.Meals;

public class MealDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }

    public bool IsActive { get; set; }
}
