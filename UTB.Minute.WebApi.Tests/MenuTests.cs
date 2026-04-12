using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Contracts.Menu;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Aspire")]
public class MenuTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public MenuTests(AspireFixture fixture)
    {
        _client = fixture.WebApiClient;
    }

    private async Task<MealDto> CreateTestMeal()
    {
        var mealDto = new CreateMealDto
        {
            Name = $"Menu Test Meal {Guid.NewGuid():N}",
            Description = "Test Description",
            Price = 100.00m
        };

        var response = await _client.PostAsJsonAsync("/meals", mealDto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MealDto>(JsonOptions))!;
    }

    [Fact]
    public async Task GetMenuItems_Returns200Ok()
    {
        var response = await _client.GetAsync("/menu-items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        Assert.NotNull(items);
    }

    [Fact]
    public async Task CreateMenuItem_WithValidData_Returns201Created()
    {
        var meal = await CreateTestMeal();

        var dto = new CreateMenuItemDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            MealId = meal.Id,
            AvailablePortions = 50
        };

        var response = await _client.PostAsJsonAsync("/menu-items", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<MenuItemDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(dto.MealId, created.MealId);
        Assert.Equal(dto.AvailablePortions, created.AvailablePortions);
    }

    [Fact]
    public async Task UpdateMenuItem_WithValidData_Returns200Ok()
    {
        var meal = await CreateTestMeal();

        var createDto = new CreateMenuItemDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            MealId = meal.Id,
            AvailablePortions = 30
        };

        var createResponse = await _client.PostAsJsonAsync("/menu-items", createDto);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<MenuItemDto>(JsonOptions);

        var updateDto = new UpdateMenuItemDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            MealId = meal.Id,
            AvailablePortions = 100
        };

        var updateResponse = await _client.PutAsJsonAsync($"/menu-items/{createdItem!.Id}", updateDto);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<MenuItemDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal(updateDto.AvailablePortions, updated.AvailablePortions);
    }

    [Fact]
    public async Task DeleteMenuItem_WithValidId_Returns204NoContent()
    {
        var meal = await CreateTestMeal();

        var createDto = new CreateMenuItemDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            MealId = meal.Id,
            AvailablePortions = 50
        };

        var createResponse = await _client.PostAsJsonAsync("/menu-items", createDto);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<MenuItemDto>(JsonOptions);

        var deleteResponse = await _client.DeleteAsync($"/menu-items/{createdItem!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteMenuItem_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/menu-items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
