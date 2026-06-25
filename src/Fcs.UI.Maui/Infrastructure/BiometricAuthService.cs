using Fcs.UI.Application.Interfaces;
using Maui.Biometric;

namespace Fcs.UI.Maui.Infrastructure;

public class BiometricAuthService : IBiometricService
{
    private readonly IBiometricAuthentication _biometric;

    public BiometricAuthService(IBiometricAuthentication biometric)
    {
        _biometric = biometric;
    }

    public async Task<bool> AuthenticateAsync(string reason = "Autentique-se para continuar")
    {
        var availability = await _biometric.CheckAvailabilityAsync();
        if (!availability.IsAvailable)
            return false;

        var result = await _biometric.AuthenticateAsync(
            new AuthenticationRequest(
                title: "Conexão Solidária",
                reason: reason),
            CancellationToken.None);

        return result.IsSuccessful;
    }

    public async Task<bool> IsAvailableAsync()
    {
        var availability = await _biometric.CheckAvailabilityAsync();
        return availability.IsAvailable;
    }
}
