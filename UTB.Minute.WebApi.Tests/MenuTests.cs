using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UTB.Minute.Contracts.Menu;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Db;
using UTB.Minute.Db.Entities;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

public class MenuTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MenuTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MinuteDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<MinuteDbContext>(options =>
                {
                    options.UseNpgsql("Host=localhost;Port=5432;Database=minute_test_db;Username=postgres;Password=postgres");
                });

                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MinuteDbContext>();
                
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        });
    }

    [Fact]
    public async Task GetMenuItems_Returns200Ok()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/menu");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateMenuItem_WithValidData_Returns201Created()
    {
        var client = _factory.CreateClient();

        var mealDto = new CreateMealDto
        {
            Name = "Test Meal",
            Description = "Test Description",
            Price = 100.00m
        };

        var mealContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(mealDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var mealResponse = await client.PostAsync("/meals", mealContent);
        var meal = await System.Text.Json.JsonSerializer.DeserializeAsync<MealDto>(
            await mealResponse.Content.ReadAsStreamAsync());

        var menuItemDto = new CreateMenuItemDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            MealId = meal.Id,
            AvailablePortions = 50
        };

        var menuContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(menuItemDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/menu", menuContent);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMenuItem_WithValidId_Returns200Ok()
    {
        var client = _factory.CreateClient();

        var mealDto = new CreateMealDto
        {
            Name = "Test Meal",
            Description = "Test Description",
            Price = 100.00m
        };

        var mealContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(mealDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var mealResponse = await client.PostAsync("/meals", mealContent);
        var meal = await System.Text.Json.JsonSerializer.DeserializeAsync<MealDto>(
            await mealResponse.Content.ReadAsStreamAsync());

        var menuItemDto = new CreateMenuItemDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            MealId = meal.Id,
            AvailablePortions = 50
        };

        var menuContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(menuItemDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var createResponse = await client.PostAsync("/menu", menuContent);
        var menuItem = await System.Text.Json.JsonSerializer.DeserializeAsync<MenuItemDto>(
            await createResponse.Content.ReadAsStreamAsync());

        var deleteResponse = await client.DeleteAsync($"/menu/{menuItem.Id}");

        Assert.Equal(System.Net.HttpStatusCode.OK, deleteResponse.StatusCode);
    }
}
