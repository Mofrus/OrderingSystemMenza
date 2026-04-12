using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UTB.Minute.Contracts.Enums;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Contracts.Menu;
using UTB.Minute.Contracts.Orders;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Aspire")]
public class OrdersTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public OrdersTests(AspireFixture fixture)
    {
        _client = fixture.WebApiClient;
    }

    private async Task<MenuItemDto> CreateTestMenuItem()
    {
        var mealDto = new CreateMealDto
        {
            Name = $"Order Test Meal {Guid.NewGuid():N}",
            Description = "Test Description",
            Price = 100.00m
        };

        var mealResponse = await _client.PostAsJsonAsync("/meals", mealDto);
        var meal = await mealResponse.Content.ReadFromJsonAsync<MealDto>(JsonOptions);

        var menuItemDto = new CreateMenuItemDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            MealId = meal!.Id,
            AvailablePortions = 50
        };

        var menuResponse = await _client.PostAsJsonAsync("/menu-items", menuItemDto);
        return (await menuResponse.Content.ReadFromJsonAsync<MenuItemDto>(JsonOptions))!;
    }

    [Fact]
    public async Task GetOrders_Returns200Ok()
    {
        var response = await _client.GetAsync("/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(JsonOptions);
        Assert.NotNull(orders);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_Returns201Created()
    {
        var menuItem = await CreateTestMenuItem();

        var orderDto = new CreateOrderDto
        {
            MenuItemId = menuItem.Id
        };

        var response = await _client.PostAsJsonAsync("/orders", orderDto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(menuItem.Id, created.MenuItemId);
        Assert.Equal(OrderStatus.Preparing, created.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidData_Returns200Ok()
    {
        var menuItem = await CreateTestMenuItem();

        var orderDto = new CreateOrderDto
        {
            MenuItemId = menuItem.Id
        };

        var orderResponse = await _client.PostAsJsonAsync("/orders", orderDto);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);

        var updateStatusDto = new UpdateOrderStatusDto
        {
            Status = OrderStatus.Ready
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/orders/{order!.Id}/status")
        {
            Content = JsonContent.Create(updateStatusDto)
        };

        var statusResponse = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

        var updated = await statusResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal(OrderStatus.Ready, updated.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_NotFound_Returns404()
    {
        var updateStatusDto = new UpdateOrderStatusDto
        {
            Status = OrderStatus.Ready
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/orders/{Guid.NewGuid()}/status")
        {
            Content = JsonContent.Create(updateStatusDto)
        };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
