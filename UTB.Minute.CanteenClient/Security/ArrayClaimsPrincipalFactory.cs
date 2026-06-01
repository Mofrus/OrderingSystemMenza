using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace UTB.Minute.CanteenClient.Security;

public class ArrayClaimsPrincipalFactory<TAccount> : AccountClaimsPrincipalFactory<TAccount> where TAccount : RemoteUserAccount
{
    public ArrayClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
    : base(accessor)
    { }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(TAccount account, RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);
        var claimsIdentity = (ClaimsIdentity?)user.Identity;

        if (account != null && claimsIdentity != null)
        {
            foreach (var kvp in account.AdditionalProperties)
            {
                var name = kvp.Key;
                var value = kvp.Value;

                if (value is JsonElement element)
                {
                    // Handle top-level array claims (e.g. plain "role" arrays)
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(name));
                        var claims = element.EnumerateArray().Select(x => new Claim(name, x.ToString()));
                        claimsIdentity.AddClaims(claims);
                    }
                    // Handle Keycloak's realm_access: { "roles": ["Cook", "Admin", ...] }
                    else if (name == "realm_access" && element.ValueKind == JsonValueKind.Object
                             && element.TryGetProperty("roles", out var rolesElement)
                             && rolesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleStr = role.GetString();
                            if (!string.IsNullOrEmpty(roleStr))
                                claimsIdentity.AddClaim(new Claim("role", roleStr));
                        }
                    }
                }
            }
        }

        return user;
    }
}