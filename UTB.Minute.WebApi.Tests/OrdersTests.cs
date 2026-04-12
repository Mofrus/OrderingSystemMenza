using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UTB.Minute.Contracts.Orders;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Contracts.Menu;
using UTB.Minute.Db;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

public class OrdersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrdersTests(WebApplicationFactory<Program> factory)
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
    public async Task CreateOrder_WithValidData_Returns201Created()
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

        var menuResponse = await client.PostAsync("/menu", menuContent);
        var menuItem = await System.Text.Json.JsonSerializer.DeserializeAsync<MenuItemDto>(
            await menuResponse.Content.ReadAsStreamAsync());

        var orderDto = new CreateOrderDto
        {
            MenuItemId = menuItem.Id
        };

        var orderContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(orderDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var orderResponse = await client.PostAsync("/orders", orderContent);

        Assert.Equal(System.Net.HttpStatusCode.Created, orderResponse.StatusCode);
    }

    [Fact]
    public async Task GetPendingOrders_Returns200Ok()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/orders/pending");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidData_Returns200Ok()
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

        var menuResponse = await client.PostAsync("/menu", menuContent);
        var menuItem = await System.Text.Json.JsonSerializer.DeserializeAsync<MenuItemDto>(
            await menuResponse.Content.ReadAsStreamAsync());

        var orderDto = new CreateOrderDto
        {
            MenuItemId = menuItem.Id
        };

        var orderContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(orderDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var orderResponse = await client.PostAsync("/orders", orderContent);
        var order = await System.Text.Json.JsonSerializer.DeserializeAsync<OrderDto>(
            await orderResponse.Content.ReadAsStreamAsync());

        var updateStatusDto = new UpdateOrderStatusDto
        {
            Status = OrderStatus.Ready
        };

        var statusContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(updateStatusDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var statusResponse = await client.PutAsync($"/orders/{order.Id}/status", statusContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, statusResponse.StatusCode);
    }
}
