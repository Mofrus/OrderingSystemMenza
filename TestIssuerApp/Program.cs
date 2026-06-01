using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text.Json;

class Program
{
    static void Main()
    {
        var token = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJKNnRyYnpVeVdKSzh3VGh1WG9xT0FRa3VvX2Z3OHczU0lTczdVMnMxdGZjIn0.eyJleHAiOjE3ODAzMjY2NzEsImlhdCI6MTc4MDMyNjM3MSwianRpIjoib25ydHJvOjRlYmI0ZTNjLWZmMzEtOWQ3OS1jMmQwLWFhZTViMjY5M2M2NyIsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6ODA4MC9yZWFsbXMvbWVuemEiLCJzdWIiOiI0NWY4MTIxMy0xZjNlLTRmZjQtODgxZS1hYjdkYjYwZTE2NTYiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJtZW56YS1jbGllbnQiLCJzaWQiOiJXLVd3TldiUmpjTEdBN042c1F1LU93Y1QiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbIioiXSwicmVhbG1fYWNjZXNzIjp7InJvbGVzIjpbIkNvb2siXX0sInNjb3BlIjoiZW1haWwgcHJvZmlsZSIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwicm9sZSI6WyJDb29rIl0sIm5hbWUiOiJDb29rIFVzZXIiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJjb29rIiwiZ2l2ZW5fbmFtZSI6IkNvb2siLCJmYW1pbHlfbmFtZSI6IlVzZXIiLCJlbWFpbCI6ImNvb2tAdXRiLmN6In0.aD5DMIQz_47m-LRbxEM0H2R5X7XNdkmMhivMAxMnHBIAfn5X-dUCrlFA_Di7gw9Jfg5ojcwbJtkI2CeelDBhG1r4CxpuglG1tB8Xdz0h2WNjOxKCiDti9OinwzM2UwftSKjHN-R1oZ-quGdPmk6-MSil-PotNQqVpR6q0havlFfrLNYsVC6eTfUvRiCEadnLFzbCcdrFI9kS0ZjV36CWK9v1NWorkDDoycfvvDZJmXnYuUMqZszFLxtgzVnpGIh2tC_CfqylQf7aTCqPfA2jmPmAf_--oa8-CXEjoU247OWKxVaUEezFPSbzSdayQv0wR-c6qCu0w2qFmdsrAgIX7g";
        var handler = new JsonWebTokenHandler();
        var tvp = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            IssuerValidator = (issuer, securityToken, validationParameters) => issuer,
            ValidateIssuerSigningKey = false,
            ValidateLifetime = false,
            RequireSignedTokens = false
        };
        var result = handler.ValidateToken(token, tvp);
        Console.WriteLine($"IsValid: {result.IsValid}");
        if (result.Exception != null) Console.WriteLine($"Exception: {result.Exception.Message}");
    }
}
