using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using UTB.Minute.AdminClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Local", options.ProviderOptions);
    options.ProviderOptions.Authority = "http://localhost:8080/realms/menza";
    options.ProviderOptions.ClientId = "menza-client";
    options.ProviderOptions.ResponseType = "code";
    options.UserOptions.RoleClaim = "role";
});

builder.Services.AddScoped<AuthorizationMessageHandler>(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>()
        .ConfigureHandler(
            authorizedUrls: new[] { "http://localhost:5000", "https://localhost:5001", "https+http://utb-minute-webapi" },
            scopes: new[] { "api" });
    return handler;
});

// Configure the named client for our API
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https+http://utb-minute-webapi");
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

await builder.Build().RunAsync();
