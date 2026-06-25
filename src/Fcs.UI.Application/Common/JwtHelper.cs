using System.Text.Json;

namespace Fcs.UI.Application.Common;

public static class JwtHelper
{
    public static JsonElement? DecodePayload(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var padded = (parts[1].Length % 4) switch
            {
                2 => parts[1] + "==",
                3 => parts[1] + "=",
                _ => parts[1]
            };

            var json = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/')));
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch { return null; }
    }

    public static bool IsExpired(string token)
    {
        var payload = DecodePayload(token);
        if (payload is null) return true;

        if (payload.Value.TryGetProperty("exp", out var exp) &&
            exp.ValueKind == JsonValueKind.Number &&
            exp.TryGetInt64(out var expUnix))
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expUnix;
        }

        return false;
    }

    public static bool HasRole(string token, string role)
    {
        var payload = DecodePayload(token);
        if (payload is null) return false;

        if (payload.Value.TryGetProperty("realm_access", out var realm) &&
            realm.TryGetProperty("roles", out var roles) &&
            roles.ValueKind == JsonValueKind.Array)
        {
            return roles.EnumerateArray().Any(r => r.GetString() == role);
        }

        return false;
    }
}
