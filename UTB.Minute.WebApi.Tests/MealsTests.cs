using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UTB.Minute.Contracts.Meals;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Aspire")]
public class MealsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;
    private readonly HttpClient _dbManagerClient;

    public MealsTests(AspireFixture fixture)
    {
        _client = fixture.WebApiClient;
        _dbManagerClient = fixture.DbManagerClient;
    }

    [Fact]
    public async Task GetMeals_Returns200Ok()
    {
        var response = await _client.GetAsync("/meals");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var meals = await response.Content.ReadFromJsonAsync<List<MealDto>>(JsonOptions);
        Assert.NotNull(meals);
    }

    [Fact]
    public async Task CreateMeal_WithValidData_Returns201Created()
    {
        var dto = new CreateMealDto
        {
            Name = "Test Meal Create",
            Description = "Test Description",
            Price = 100.00m
        };

        var response = await _client.PostAsJsonAsync("/meals", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<MealDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(dto.Name, created.Name);
        Assert.Equal(dto.Description, created.Description);
        Assert.Equal(dto.Price, created.Price);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task UpdateMeal_WithValidData_Returns200Ok()
    {
        var createDto = new CreateMealDto
        {
            Name = "Meal To Update",
            Description = "Original Description",
            Price = 100.00m
        };

        var createResponse = await _client.PostAsJsonAsync("/meals", createDto);
        var createdMeal = await createResponse.Content.ReadFromJsonAsync<MealDto>(JsonOptions);

        var updateDto = new UpdateMealDto
        {
            Name = "Updated Meal",
            Description = "Updated Description",
            Price = 150.00m,
            IsActive = true
        };

        var updateResponse = await _client.PutAsJsonAsync($"/meals/{createdMeal!.Id}", updateDto);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<MealDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal(updateDto.Name, updated.Name);
        Assert.Equal(updateDto.Price, updated.Price);
    }

    [Fact]
    public async Task DeactivateMeal_Returns204NoContent()
    {
        var createDto = new CreateMealDto
        {
            Name = "Meal To Deactivate",
            Description = "Will be deactivated",
            Price = 120.00m
        };

        var createResponse = await _client.PostAsJsonAsync("/meals", createDto);
        var createdMeal = await createResponse.Content.ReadFromJsonAsync<MealDto>(JsonOptions);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/meals/{createdMeal!.Id}/deactivate");
        var deactivateResponse = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        // Verify meal is deactivated by listing meals
        var listResponse = await _client.GetAsync("/meals");
        var meals = await listResponse.Content.ReadFromJsonAsync<List<MealDto>>(JsonOptions);
        var deactivatedMeal = meals!.FirstOrDefault(m => m.Id == createdMeal.Id);
        Assert.NotNull(deactivatedMeal);
        Assert.False(deactivatedMeal.IsActive);
    }

    [Fact]
    public async Task UpdateMeal_NotFound_Returns404()
    {
        var updateDto = new UpdateMealDto
        {
            Name = "Non Existent",
            Description = "Does not exist",
            Price = 100.00m,
            IsActive = true
        };

        var response = await _client.PutAsJsonAsync($"/meals/{Guid.NewGuid()}", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
