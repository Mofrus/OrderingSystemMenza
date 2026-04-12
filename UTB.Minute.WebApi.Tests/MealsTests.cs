using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UTB.Minute.Contracts.Meals;
using UTB.Minute.Db;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

public class MealsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MealsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetMeals_Returns200Ok()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/meals");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateMeal_WithValidData_Returns201Created()
    {
        var client = _factory.CreateClient();
        var dto = new CreateMealDto
        {
            Name = "Test Meal",
            Description = "Test Description",
            Price = 100.00m
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(dto),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/meals", content);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMeal_WithValidData_Returns200Ok()
    {
        var client = _factory.CreateClient();
        
        var createDto = new CreateMealDto
        {
            Name = "Original Meal",
            Description = "Original Description",
            Price = 100.00m
        };

        var createContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(createDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var createResponse = await client.PostAsync("/meals", createContent);
        var createdMeal = await System.Text.Json.JsonSerializer.DeserializeAsync<MealDto>(
            await createResponse.Content.ReadAsStreamAsync());

        var updateDto = new UpdateMealDto
        {
            Name = "Updated Meal",
            Description = "Updated Description",
            Price = 150.00m,
            IsActive = true
        };

        var updateContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(updateDto),
            System.Text.Encoding.UTF8,
            "application/json");

        var updateResponse = await client.PutAsync($"/meals/{createdMeal.Id}", updateContent);

        Assert.Equal(System.Net.HttpStatusCode.OK, updateResponse.StatusCode);
    }
}
