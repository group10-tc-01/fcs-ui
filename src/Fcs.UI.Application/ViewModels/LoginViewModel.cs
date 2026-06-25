using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly NavigationManager _navigation;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel(IAuthService authService, NavigationManager navigation)
    {
        _authService = authService;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        var email = Email?.Trim() ?? string.Empty;
        var password = Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "E-mail é obrigatório";
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Senha é obrigatória";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var request = new LoginRequest(email, password);
            var result = await _authService.LoginAsync(request);

            if (result.IsFailed)
                ErrorMessage = result.Errors.First().Message;
            else
                _navigation.NavigateTo("/campaigns");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoToRegister()
    {
        _navigation.NavigateTo("/register");
    }
}
