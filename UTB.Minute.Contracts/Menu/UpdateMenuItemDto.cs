namespace UTB.Minute.Contracts.Menu;

public class UpdateMenuItemDto
{
    public DateOnly Date { get; set; }

    public Guid MealId { get; set; }

    public int AvailablePortions { get; set; }
}
