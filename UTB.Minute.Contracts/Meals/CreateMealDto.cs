namespace UTB.Minute.Contracts.Meals;

public class CreateMealDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
}
