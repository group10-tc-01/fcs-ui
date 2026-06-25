using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.Common;
using Fcs.UI.Application.Interfaces;
using System.Text.Json;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class ProfileViewModel : ObservableObject
{
    private readonly ITokenStorage _tokenStorage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _fullName;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _userId;

    [ObservableProperty]
    private IReadOnlyList<string> _roles = [];

    public ProfileViewModel(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var token = await _tokenStorage.GetAccessTokenAsync();
            if (token is null) return;

            var payload = JwtHelper.DecodePayload(token);
            if (payload is null) return;

            FullName = GetString(payload.Value, "name");
            Email = GetString(payload.Value, "email") ?? GetString(payload.Value, "preferred_username");
            UserId = GetString(payload.Value, "sub");
            Roles = GetRoles(payload.Value);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string? GetString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;
    }

    private static readonly HashSet<string> AppRoles = ["Doador", "GestorONG"];

    private static IReadOnlyList<string> GetRoles(JsonElement element)
    {
        if (!element.TryGetProperty("realm_access", out var realm) ||
            !realm.TryGetProperty("roles", out var roles) ||
            roles.ValueKind != JsonValueKind.Array)
            return [];

        return roles.EnumerateArray()
            .Select(r => r.GetString())
            .Where(r => r is not null && AppRoles.Contains(r))
            .Cast<string>()
            .ToList();
    }
}
