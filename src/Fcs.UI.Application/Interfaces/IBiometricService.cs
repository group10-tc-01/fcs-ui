namespace Fcs.UI.Application.Interfaces;

public interface IBiometricService
{
    Task<bool> AuthenticateAsync(string reason = "Autentique-se para continuar");
    Task<bool> IsAvailableAsync();
}
