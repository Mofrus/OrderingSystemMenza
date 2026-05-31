using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

        // Wait a little bit for Keycloak to be fully ready
        await Task.Delay(10000);

        try 
        {
            var keycloakClient = App.CreateHttpClient("keycloak");
            
            // Retry logic for CI environments
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var tokenResponse = await keycloakClient.PostAsync("/realms/menza/protocol/openid-connect/token", new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", "menza-client"),
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", "admin"),
                        new KeyValuePair<string, string>("password", "admin")
                    }));

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        var json = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
                        var token = json.GetProperty("access_token").GetString();
                        WebApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        break;
                    }
                }
                catch
                {
                    // Ignore transient network errors during startup
                }
                
                await Task.Delay(3000);
            }
        }
        catch (Exception)
        {
            // Ignore if Keycloak isn't available or fails, tests will fail naturally
        }

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
