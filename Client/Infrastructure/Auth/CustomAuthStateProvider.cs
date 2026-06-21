using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Infrastructure.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(ITokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(_anonymous);
            }

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            if (IsTokenExpired(identity))
            {
                // Token is expired. Silently return anonymous so Blazor thinks we're logged out.
                // The JwtAuthHandler will attempt silent refresh when an API request goes out.
                return new AuthenticationState(_anonymous);
            }

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(authState);
    }

    private static bool IsTokenExpired(ClaimsIdentity identity)
    {
        var expClaim = identity.FindFirst("exp")?.Value;
        if (string.IsNullOrEmpty(expClaim)) return true;

        if (long.TryParse(expClaim, out var expSeconds))
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
            // Add a 10 seconds grace period
            return expirationTime.AddSeconds(-10) <= DateTimeOffset.UtcNow;
        }

        return true;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs == null) return [];

        var claims = new List<Claim>();

        foreach (var kvp in keyValuePairs)
        {
            var valueStr = kvp.Value?.ToString() ?? "";
            
            if (kvp.Value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        var val = item.ToString();
                        claims.Add(new Claim(kvp.Key, val));
                        if (kvp.Key == "role" || kvp.Key == ClaimTypes.Role)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, val));
                        }
                    }
                    continue;
                }
                else
                {
                    valueStr = element.ToString();
                }
            }

            claims.Add(new Claim(kvp.Key, valueStr));
            
            if (kvp.Key == "role" && valueStr != "")
            {
                claims.Add(new Claim(ClaimTypes.Role, valueStr));
            }
            if (kvp.Key == "unique_name" || kvp.Key == "name")
            {
                claims.Add(new Claim(ClaimTypes.Name, valueStr));
            }
            if (kvp.Key == "sub" || kvp.Key == "nameid")
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, valueStr));
            }
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
