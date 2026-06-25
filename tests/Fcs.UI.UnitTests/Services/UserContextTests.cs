using Fcs.UI.Application.Interfaces;
using Fcs.UI.Application.Services;
using FluentAssertions;
using Moq;
using System.Text;

namespace Fcs.UI.UnitTests.Services;

public class UserContextTests
{
    private static string CreateJwt(string payloadJson)
    {
        var header = Base64UrlEncode(Encoding.UTF8.GetBytes(@"{""alg"":""HS256"",""typ"":""JWT""}"));
        var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        return $"{header}.{payload}.fakesig";
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static Mock<ITokenStorage> StorageWithToken(string? token)
    {
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync(token);
        return storage;
    }

    [Fact]
    public async Task InitializeAsync_WithValidToken_ParsesClaims()
    {
        var token = CreateJwt(@"{""sub"":""user-123"",""email"":""user@test.com"",""name"":""Test User"",""preferred_username"":""testuser""}");
        var ctx = new UserContext(StorageWithToken(token).Object);

        await ctx.InitializeAsync();

        ctx.IsAuthenticated.Should().BeTrue();
        ctx.UserId.Should().Be("user-123");
        ctx.Email.Should().Be("user@test.com");
        ctx.UserName.Should().Be("testuser");
        ctx.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WithRole_IsAdminTrue()
    {
        var token = CreateJwt(@"{""sub"":""admin-1"",""role"":""GestorONG""}");
        var ctx = new UserContext(StorageWithToken(token).Object);

        await ctx.InitializeAsync();

        ctx.IsAdmin.Should().BeTrue();
        ctx.Role.Should().Be("GestorONG");
    }

    [Fact]
    public async Task InitializeAsync_WithRolesArray_ExtractsRoles()
    {
        var token = CreateJwt(@"{""sub"":""user-1"",""roles"":[""Doador"",""user""]}");
        var ctx = new UserContext(StorageWithToken(token).Object);

        await ctx.InitializeAsync();

        ctx.IsInRole("Doador").Should().BeTrue();
        ctx.IsInRole("user").Should().BeTrue();
        ctx.IsInRole("GestorONG").Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WithRealmAccess_ExtractsRoles()
    {
        var token = CreateJwt(@"{""sub"":""admin-2"",""realm_access"":{""roles"":[""GestorONG"",""offline_access""]}}");
        var ctx = new UserContext(StorageWithToken(token).Object);

        await ctx.InitializeAsync();

        ctx.IsAdmin.Should().BeTrue();
        ctx.IsInRole("offline_access").Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_WithResourceAccess_ExtractsClientRoles()
    {
        var token = CreateJwt(@"{""sub"":""user-3"",""resource_access"":{""account"":{""roles"":[""manage-account"",""view-profile""]},""fcs-app"":{""roles"":[""Doador""]}}}");
        var ctx = new UserContext(StorageWithToken(token).Object);

        await ctx.InitializeAsync();

        ctx.IsInRole("Doador").Should().BeTrue();
        ctx.IsInRole("manage-account").Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_NullToken_NotAuthenticated()
    {
        var ctx = new UserContext(StorageWithToken(null).Object);

        await ctx.InitializeAsync();

        ctx.IsAuthenticated.Should().BeFalse();
        ctx.UserId.Should().BeNull();
        ctx.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_InvalidToken_NotAuthenticated()
    {
        var ctx = new UserContext(StorageWithToken("not-a-jwt").Object);

        await ctx.InitializeAsync();

        ctx.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_CalledTwice_ReReadsToken()
    {
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync(CreateJwt(@"{""sub"":""1""}"));
        var ctx = new UserContext(storage.Object);

        await ctx.InitializeAsync();
        await ctx.InitializeAsync();

        storage.Verify(s => s.GetAccessTokenAsync(), Times.Exactly(2));
    }

    [Fact]
    public void IsInRole_WithNoPrincipal_ReturnsFalse()
    {
        var ctx = new UserContext(new Mock<ITokenStorage>().Object);

        ctx.IsInRole("Doador").Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_TokenStorageThrows_NotAuthenticated()
    {
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ThrowsAsync(new InvalidOperationException());
        var ctx = new UserContext(storage.Object);

        await ctx.InitializeAsync();

        ctx.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task IsInRole_CaseInsensitive_Match()
    {
        var token = CreateJwt(@"{""role"":""gestorong""}");
        var ctx = new UserContext(StorageWithToken(token).Object);

        await ctx.InitializeAsync();

        ctx.IsAdmin.Should().BeTrue();
        ctx.IsInRole("GestorONG").Should().BeTrue();
    }

    [Fact]
    public async Task ExtractRoles_DeduplicatesDuplicateRoles()
    {
        var token = CreateJwt(@"{""sub"":""user-4"",""role"":""Doador"",""realm_access"":{""roles"":[""Doador"",""Doador""]}}");
        var ctx = new UserContext(StorageWithToken(token).Object);

        await ctx.InitializeAsync();

        ctx.IsInRole("Doador").Should().BeTrue();
    }

    [Fact]
    public async Task EmptyToken_NotAuthenticated()
    {
        var ctx = new UserContext(StorageWithToken("").Object);

        await ctx.InitializeAsync();

        ctx.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task TwoPartToken_NotAuthenticated()
    {
        var ctx = new UserContext(StorageWithToken("header.payload").Object);

        await ctx.InitializeAsync();

        ctx.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_MissingSub_PropertiesNull()
    {
        var token = CreateJwt(@"{""email"":""x@y.com""}");
        var ctx = new UserContext(StorageWithToken(token).Object);

        await ctx.InitializeAsync();

        ctx.IsAuthenticated.Should().BeTrue();
        ctx.UserId.Should().BeNull();
        ctx.Email.Should().Be("x@y.com");
    }
}
