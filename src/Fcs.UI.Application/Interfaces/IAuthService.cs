using Fcs.UI.Application.DTOs;
using FluentResults;

namespace Fcs.UI.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthSessionDto>> LoginAsync(LoginRequest request);
    Task<Result<RegisterDonorDto>> RegisterAsync(RegisterRequest request);
    Task<Result> RefreshTokenAsync();
    Task LogoutAsync();
}

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterRequest(string FullName, string Email, string Cpf, string Password);
