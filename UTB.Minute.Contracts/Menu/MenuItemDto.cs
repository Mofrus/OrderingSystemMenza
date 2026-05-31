namespace UTB.Minute.Contracts.Menu;

public class MenuItemDto
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public Guid MealId { get; set; }
    
    public string MealName { get; set; } = string.Empty;

    public int AvailablePortions { get; set; }
}
