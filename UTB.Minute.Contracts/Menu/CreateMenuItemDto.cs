namespace UTB.Minute.Contracts.Menu;

public class CreateMenuItemDto
{
    public DateOnly Date { get; set; }

    public Guid MealId { get; set; }

    public int AvailablePortions { get; set; }
}
