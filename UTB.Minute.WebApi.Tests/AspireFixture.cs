using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

public class AspireFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;
    public HttpClient WebApiClient { get; private set; } = null!;
    public HttpClient DbManagerClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.UTB_Minute_AppHost>();

        App = await builder.BuildAsync();
        await App.StartAsync();

        WebApiClient = App.CreateHttpClient("utb-minute-webapi");
        DbManagerClient = App.CreateHttpClient("utb-minute-dbmanager");

        // Reset and seed the database before running tests
        var resetResponse = await DbManagerClient.PostAsync("/db/reset-seed", null);
        resetResponse.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        await App.DisposeAsync();
    }
}

[CollectionDefinition("Aspire")]
public class AspireCollection : ICollectionFixture<AspireFixture>
{
}
