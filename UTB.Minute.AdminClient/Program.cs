using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using UTB.Minute.AdminClient;

using UTB.Minute.AdminClient.Security;

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
})
.AddAccountClaimsPrincipalFactory<ArrayClaimsPrincipalFactory<RemoteUserAccount>>();

var apiUrl = builder.Configuration["services:utb-minute-webapi:http:0"] ?? 
             builder.Configuration["services:utb-minute-webapi:https:0"] ?? 
             "http://localhost:5555"; // Fallback

builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

// Configure the named client for our API
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(apiUrl);
})
.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

await builder.Build().RunAsync();
