using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly NavigationManager _navigation;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _cpf = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    public RegisterViewModel(IAuthService authService, NavigationManager navigation)
    {
        _authService = authService;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Senhas não conferem";
            return;
        }

        IsLoading = true;

        try
        {
            var request = new RegisterRequest(FullName, Email, Cpf, Password);
            var result = await _authService.RegisterAsync(request);

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
}
