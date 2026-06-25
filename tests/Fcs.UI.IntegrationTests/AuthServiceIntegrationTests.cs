using Fcs.UI.Application.Interfaces;
using Fcs.UI.Infrastructure.Http;
using Fcs.UI.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Refit;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Fcs.UI.IntegrationTests;

public class AuthServiceIntegrationTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly Mock<ITokenStorage> _tokenStorage;
    private readonly IAuthService _authService;

    public AuthServiceIntegrationTests()
    {
        _server = WireMockServer.Start();
        _tokenStorage = new Mock<ITokenStorage>();

        var authApi = RestService.For<IAuthApi>(_server.Url!);
        _authService = new AuthService(authApi, _tokenStorage.Object);
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsOkAndSavesToken()
    {
        _server.Given(Request.Create().WithPath("/api/v1/auth/login").UsingPost())
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"success":true,"data":{"accessToken":"jwt-token","refreshToken":"refresh-token","expiresIn":3600,"tokenType":"Bearer"},"message":null}"""));

        var result = await _authService.LoginAsync(new LoginRequest("john@test.com", "pass"));

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        _tokenStorage.Verify(s => s.SaveTokensAsync("jwt-token", "refresh-token"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsFail()
    {
        _server.Given(Request.Create().WithPath("/api/v1/auth/login").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(401));

        var result = await _authService.LoginAsync(new LoginRequest("wrong", "creds"));

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_ValidData_ReturnsOk()
    {
        _server.Given(Request.Create().WithPath("/api/v1/auth/register/donor").UsingPost())
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"success":true,"data":{"id":"b3d4e5f6-0000-0000-0000-000000000001","fullName":"John","email":"john@test.com","cpf":"12345678909"},"message":null}"""));

        var result = await _authService.RegisterAsync(
            new RegisterRequest("John", "john@test.com", "12345678909", "pass"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_ServerError_ReturnsFail()
    {
        _server.Given(Request.Create().WithPath("/api/v1/auth/register/donor").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(500));

        var result = await _authService.RegisterAsync(
            new RegisterRequest("John", "john@test.com", "12345678909", "pass"));

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithStoredToken_RefreshesAndSaves()
    {
        _tokenStorage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync("old-token");

        _server.Given(Request.Create().WithPath("/api/v1/auth/refresh").UsingPost())
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"success":true,"data":{"accessToken":"new-token","refreshToken":"new-refresh","expiresIn":3600,"tokenType":"Bearer"},"message":null}"""));

        var result = await _authService.RefreshTokenAsync();

        result.IsSuccess.Should().BeTrue();
        _tokenStorage.Verify(s => s.SaveTokensAsync("new-token", "new-refresh"), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_NoStoredToken_ReturnsFail()
    {
        _tokenStorage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync((string?)null);

        var result = await _authService.RefreshTokenAsync();

        result.IsFailed.Should().BeTrue();
        _tokenStorage.Verify(s => s.SaveTokensAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ClearsTokens()
    {
        _server.Given(Request.Create().WithPath("/api/v1/auth/logout").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200));

        await _authService.LogoutAsync();

        _tokenStorage.Verify(s => s.ClearTokensAsync(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ApiError_StillClearsTokens()
    {
        _server.Given(Request.Create().WithPath("/api/v1/auth/logout").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(500));

        await _authService.LogoutAsync();

        _tokenStorage.Verify(s => s.ClearTokensAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_NetworkError_ReturnsFail()
    {
        var offlineServer = WireMockServer.Start();
        offlineServer.Given(Request.Create().WithPath("/api/v1/auth/login").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200));
        offlineServer.Stop();

        var offlineApi = RestService.For<IAuthApi>(offlineServer.Url!);
        var offlineAuth = new AuthService(offlineApi, _tokenStorage.Object);

        var result = await offlineAuth.LoginAsync(new LoginRequest("john@test.com", "pass"));

        result.IsFailed.Should().BeTrue();
    }
}
