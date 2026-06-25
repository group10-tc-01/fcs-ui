using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Infrastructure.Http;
using FluentResults;
using System.Text.Json;

namespace Fcs.UI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAuthApi _authApi;
    private readonly ITokenStorage _tokenStorage;

    public AuthService(IAuthApi authApi, ITokenStorage tokenStorage)
    {
        _authApi = authApi;
        _tokenStorage = tokenStorage;
    }

    public async Task<Result<AuthSessionDto>> LoginAsync(LoginRequest request)
    {
        try
        {
            var envelope = await _authApi.LoginAsync(request);
            if (envelope is null || !envelope.Success || envelope.Data is null)
                return Result.Fail(envelope?.Message ?? "Login failed");

            var session = envelope.Data;
            await _tokenStorage.SaveTokensAsync(session.AccessToken, session.RefreshToken);
            return Result.Ok(session);
        }
        catch (Refit.ApiException ex)
        {
            var message = TryExtractErrorMessage(ex.Content) ?? ex.Message;
            return Result.Fail(message);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<RegisterDonorDto>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var envelope = await _authApi.RegisterAsync(request);
            if (envelope is null || !envelope.Success || envelope.Data is null)
                return Result.Fail(envelope?.Message ?? "Registration failed");

            return Result.Ok(envelope.Data);
        }
        catch (Refit.ApiException ex)
        {
            var message = TryExtractErrorMessage(ex.Content) ?? ex.Message;
            return Result.Fail(message);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private static string? TryExtractErrorMessage(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            var envelope = JsonSerializer.Deserialize<ApiEnvelope<string>>(content);
            if (envelope?.Message is not null)
                return envelope.Message;
        }
        catch
        {
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("title", out var title))
                return title.GetString();

            if (root.TryGetProperty("errors", out var errors))
            {
                var msgs = errors.EnumerateObject()
                    .SelectMany(e => e.Value.EnumerateArray().Select(v => v.GetString()))
                    .Where(m => m is not null);
                return string.Join("; ", msgs);
            }

            if (root.TryGetProperty("message", out var msg))
                return msg.GetString();
        }
        catch
        {
        }

        return null;
    }

    public async Task<Result> RefreshTokenAsync()
    {
        try
        {
            var token = await _tokenStorage.GetAccessTokenAsync();
            if (token is null)
                return Result.Fail("Not authenticated");

            var envelope = await _authApi.RefreshTokenAsync();
            if (envelope is null || !envelope.Success || envelope.Data is null)
                return Result.Fail(envelope?.Message ?? "Refresh failed");

            var session = envelope.Data;
            await _tokenStorage.SaveTokensAsync(session.AccessToken, session.RefreshToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _authApi.LogoutAsync();
        }
        catch
        {
        }
        finally
        {
            await _tokenStorage.ClearTokensAsync();
        }
    }
}
