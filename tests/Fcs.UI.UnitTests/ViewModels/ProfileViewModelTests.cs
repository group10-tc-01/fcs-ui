using Fcs.UI.Application.Interfaces;
using Fcs.UI.Application.ViewModels;
using FluentAssertions;
using Moq;
using System.Text;

namespace Fcs.UI.UnitTests.ViewModels;

public class ProfileViewModelTests
{
    private static string CreateTestToken(string payloadJson)
    {
        var header = Convert.ToBase64String("""{"alg":"none"}"""u8);
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));
        return $"{header}.{payload}.sig";
    }

    [Fact]
    public async Task LoadAsync_WithValidToken_PopulatesProperties()
    {
        var token = CreateTestToken("""
            {
                "name": "João Silva",
                "email": "joao@test.com",
                "sub": "abc-123",
                "realm_access": { "roles": ["Doador", "user"] }
            }
            """);

        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync(token);

        var vm = new ProfileViewModel(storage.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.FullName.Should().Be("João Silva");
        vm.Email.Should().Be("joao@test.com");
        vm.UserId.Should().Be("abc-123");
        vm.Roles.Should().BeEquivalentTo(["Doador"]);
    }

    [Fact]
    public async Task LoadAsync_NoToken_SetsNullProperties()
    {
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync((string?)null);

        var vm = new ProfileViewModel(storage.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.FullName.Should().BeNull();
        vm.Email.Should().BeNull();
        vm.UserId.Should().BeNull();
        vm.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_NoRealmAccess_ReturnsEmptyRoles()
    {
        var token = CreateTestToken("""
            {
                "name": "Maria",
                "email": "maria@test.com",
                "sub": "def-456"
            }
            """);

        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync(token);

        var vm = new ProfileViewModel(storage.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.Roles.Should().BeEmpty();
        vm.FullName.Should().Be("Maria");
    }

    [Fact]
    public async Task LoadAsync_FallbackToPreferredUsername()
    {
        var token = CreateTestToken("""
            {
                "preferred_username": "fallback@test.com",
                "sub": "ghi-789"
            }
            """);

        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync(token);

        var vm = new ProfileViewModel(storage.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.Email.Should().Be("fallback@test.com");
        vm.FullName.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_FiltersOutKeycloakTechnicalRoles()
    {
        var token = CreateTestToken("""
            {
                "name": "Admin",
                "email": "admin@test.com",
                "sub": "jkl-012",
                "realm_access": { "roles": ["GestorONG", "offline_access", "uma_authorization", "default-roles-fcs-solidarity"] }
            }
            """);

        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync(token);

        var vm = new ProfileViewModel(storage.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.Roles.Should().BeEquivalentTo(["GestorONG"]);
    }

    [Fact]
    public async Task LoadAsync_InvalidToken_DoesNotThrow()
    {
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync("not.a.jwt");

        var vm = new ProfileViewModel(storage.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.FullName.Should().BeNull();
    }
}
