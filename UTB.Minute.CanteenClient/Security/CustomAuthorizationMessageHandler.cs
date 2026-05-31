using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace UTB.Minute.CanteenClient.Security;

public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public CustomAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigation, IConfiguration config)
        : base(provider, navigation)
    {
        var apiUrl = config["services:utb-minute-webapi:http:0"] ?? 
                     config["services:utb-minute-webapi:https:0"] ?? 
                     "http://localhost:5555";
                     
        ConfigureHandler(
            authorizedUrls: new[] { apiUrl, "http://localhost:5555", "https://localhost:5556" });
    }
}
