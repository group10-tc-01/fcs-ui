using Fcs.UI.Application.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace Fcs.UI.Application.Services;

public sealed class UserContext : IUserContext
{
    private readonly ITokenStorage _tokenStorage;
    private ClaimsPrincipal? _principal;

    public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated ?? false;
    public bool IsAdmin => IsInRole("GestorONG");
    public string? UserId => FindFirst(ClaimTypes.NameIdentifier);
    public string? UserName => FindFirst("preferred_username");
    public string? Email => FindFirst(ClaimTypes.Email);
    public string? Role => FindFirst(ClaimTypes.Role);

    public UserContext(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var token = await _tokenStorage.GetAccessTokenAsync();
            _principal = ParseToken(token);
        }
        catch
        {
            _principal = null;
        }
    }

    public bool IsInRole(string role)
    {
        if (_principal is null)
            return false;

        return _principal.IsInRole(role) ||
               _principal.FindAll(ClaimTypes.Role).Any(c =>
                   c.Value.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    private static ClaimsPrincipal? ParseToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var payload = Base64UrlDecode(parts[1]);
            var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);

            if (claims is null)
                return null;

            var claimList = new List<Claim>();

            foreach (var entry in claims)
            {
                if (entry.Value.ValueKind == JsonValueKind.String)
                {
                    var claimType = MapToClaimType(entry.Key);
                    claimList.Add(new Claim(claimType, entry.Value.GetString()!));
                }
                else if (entry.Value.ValueKind == JsonValueKind.Array && entry.Key == "roles")
                {
                    foreach (var role in entry.Value.EnumerateArray().Select(r => r.GetString()))
                    {
                        if (role is not null)
                            claimList.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            var roleClaims = ExtractRoles(claims);
            foreach (var role in roleClaims)
                claimList.Add(new Claim(ClaimTypes.Role, role));

            return new ClaimsPrincipal(
                new ClaimsIdentity(claimList, "jwt"));
        }
        catch
        {
            return null;
        }
    }

    private static List<string> ExtractRoles(Dictionary<string, JsonElement> claims)
    {
        var roles = new List<string>();

        var roleKeys = new[] { "role", "roles", "realm_access", "resource_access" };

        foreach (var key in roleKeys)
        {
            if (!claims.TryGetValue(key, out var element))
                continue;

            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    roles.Add(element.GetString()!);
                    break;

                case JsonValueKind.Array:
                    roles.AddRange(element.EnumerateArray()
                        .Select(r => r.GetString())
                        .Where(r => r is not null)!);
                    break;

                case JsonValueKind.Object when key == "realm_access":
                    if (element.TryGetProperty("roles", out var realmRoles) &&
                        realmRoles.ValueKind == JsonValueKind.Array)
                    {
                        roles.AddRange(realmRoles.EnumerateArray()
                            .Select(r => r.GetString())
                            .Where(r => r is not null)!);
                    }
                    break;

                case JsonValueKind.Object when key == "resource_access":
                    foreach (var client in element.EnumerateObject())
                    {
                        if (client.Value.TryGetProperty("roles", out var clientRoles) &&
                            clientRoles.ValueKind == JsonValueKind.Array)
                        {
                            roles.AddRange(clientRoles.EnumerateArray()
                                .Select(r => r.GetString())
                                .Where(r => r is not null)!);
                        }
                    }
                    break;
            }
        }

        return roles.Distinct().ToList();
    }

    private static string MapToClaimType(string key)
    {
        return key switch
        {
            "sub" => ClaimTypes.NameIdentifier,
            "email" => ClaimTypes.Email,
            "name" => ClaimTypes.Name,
            "preferred_username" => "preferred_username",
            "role" => ClaimTypes.Role,
            _ => key
        };
    }

    private string? FindFirst(string claimType)
    {
        return _principal?.FindFirst(claimType)?.Value;
    }

    private static string Base64UrlDecode(string input)
    {
        var padded = (input.Length % 4) switch
        {
            2 => input + "==",
            3 => input + "=",
            _ => input
        };

        var base64 = padded.Replace('-', '+').Replace('_', '/');
        var bytes = Convert.FromBase64String(base64);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
