using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using UTB.Minute.CanteenClient;
using UTB.Minute.CanteenClient.Services;
using UTB.Minute.CanteenClient.Security;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Local", options.ProviderOptions);
    options.ProviderOptions.Authority = "http://localhost:8080/realms/menza";
    options.ProviderOptions.ClientId = "menza-client";
    options.ProviderOptions.ResponseType = "code";
    options.UserOptions.RoleClaim = "role";
})
.AddAccountClaimsPrincipalFactory<ArrayClaimsPrincipalFactory<RemoteUserAccount>>();

var apiUrl = builder.Configuration["services:utb-minute-webapi:http:0"] ?? 
             builder.Configuration["services:utb-minute-webapi:https:0"] ?? 
             "http://localhost:5555"; // Fallback

builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

// Configure the authenticated client for cooks
builder.Services.AddHttpClient("AuthAPI", client =>
{
    client.BaseAddress = new Uri(apiUrl);
})
.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

// Configure public client for anonymous student actions
builder.Services.AddHttpClient("PublicAPI", client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// Default HttpClient injection will use PublicAPI
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("PublicAPI"));

// Přidání SSE služby
builder.Services.AddScoped<SseNotificationService>();

await builder.Build().RunAsync();
