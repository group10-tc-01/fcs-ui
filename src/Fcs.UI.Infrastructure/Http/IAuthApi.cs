using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using Refit;

namespace Fcs.UI.Infrastructure.Http;

public interface IAuthApi
{
    [Post("/api/v1/auth/login")]
    Task<ApiEnvelope<AuthSessionDto>> LoginAsync([Body] LoginRequest request);

    [Post("/api/v1/auth/register/donor")]
    Task<ApiEnvelope<RegisterDonorDto>> RegisterAsync([Body] RegisterRequest request);

    [Post("/api/v1/auth/refresh")]
    Task<ApiEnvelope<AuthSessionDto>> RefreshTokenAsync();

    [Post("/api/v1/auth/logout")]
    Task LogoutAsync();
}
