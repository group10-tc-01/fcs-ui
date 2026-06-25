using Fcs.UI.Application.Commands;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Application.Queries;
using Fcs.UI.Application.ViewModels;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Components;
using Moq;

namespace Fcs.UI.UnitTests.ViewModels;

public class DonationViewModelTests
{
    [Fact]
    public async Task SubmitAsync_Online_SendsCommand()
    {
        var mediator = new Mock<IMediator>();
        var connectivity = new Mock<IConnectivityService>();
        var campaignRepo = new Mock<ICampaignRepository>();
        
        connectivity.Setup(c => c.IsConnected).Returns(true);
        mediator
            .Setup(m => m.Send(It.IsAny<SubmitDonationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new DonationResponse(Guid.NewGuid(), "Pending")));

        var vm = new DonationViewModel(mediator.Object, connectivity.Object, campaignRepo.Object);

        vm.Amount = 100m;
        vm.DonorCpf = "52998224725";
        await vm.SubmitCommand.ExecuteAsync(null);

        vm.IsSubmitting.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();

        campaignRepo.Verify(r => r.InvalidateActiveCache(), Times.Once);
        mediator.Verify(m => m.Send(It.IsAny<SubmitDonationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_Offline_ShowsError()
    {
        var mediator = new Mock<IMediator>();
        var connectivity = new Mock<IConnectivityService>();
        var campaignRepo = new Mock<ICampaignRepository>();
        connectivity.Setup(c => c.IsConnected).Returns(false);

        var vm = new DonationViewModel(mediator.Object, connectivity.Object, campaignRepo.Object);
        await vm.SubmitCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Sem conexão com a internet");
        mediator.Verify(m => m.Send(It.IsAny<SubmitDonationCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitAsync_HandlerFailure_ShowsError()
    {
        var mediator = new Mock<IMediator>();
        var connectivity = new Mock<IConnectivityService>();
        var campaignRepo = new Mock<ICampaignRepository>();
        connectivity.Setup(c => c.IsConnected).Returns(true);
        mediator
            .Setup(m => m.Send(It.IsAny<SubmitDonationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("API error"));

        var vm = new DonationViewModel(mediator.Object, connectivity.Object, campaignRepo.Object);
        vm.Amount = 50m;
        vm.DonorCpf = "52998224725";
        await vm.SubmitCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("API error");
    }
}

public class CampaignListViewModelTests
{
    [Fact]
    public async Task LoadAsync_Success_PopulatesCampaigns()
    {
        var mediator = new Mock<IMediator>();
        var campaigns = new List<CampaignDto>
        {
            new(Guid.NewGuid(), "Camp1", "Desc1", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 1000, "Active", 500, Guid.NewGuid(), DateTime.UtcNow, null),
            new(Guid.NewGuid(), "Camp2", "Desc2", DateTime.UtcNow, DateTime.UtcNow, 2000, "Completed", 2000, Guid.NewGuid(), DateTime.UtcNow, null),
        };
        mediator
            .Setup(m => m.Send(It.IsAny<GetCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<CampaignDto>>(campaigns));

        var vm = new CampaignListViewModel(mediator.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.Campaigns.Should().HaveCount(2);
        vm.Campaigns[0].Title.Should().Be("Camp2");
        vm.Campaigns[1].Title.Should().Be("Camp1");
    }

    [Fact]
    public async Task LoadAsync_Failure_ShowsError()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyList<CampaignDto>>("Network error"));

        var vm = new CampaignListViewModel(mediator.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().Be("Network error");
        vm.Campaigns.Should().BeEmpty();
    }
}

public class LoginViewModelTests
{
    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            // No-op for tests — navigation tracking not needed
        }
    }

    private static NavigationManager CreateNav() => new TestNavigationManager();

    [Fact]
    public async Task LoginAsync_Success_ClearsError()
    {
        var auth = new Mock<IAuthService>();
        auth
            .Setup(a => a.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(Result.Ok(new AuthSessionDto("access-token", "refresh-token", 3600, "Bearer")));

        var vm = new LoginViewModel(auth.Object, CreateNav());
        vm.Email = "user@example.com";
        vm.Password = "pass";
        await vm.LoginCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_Failure_ShowsError()
    {
        var auth = new Mock<IAuthService>();
        auth
            .Setup(a => a.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(Result.Fail("Invalid credentials"));

        var vm = new LoginViewModel(auth.Object, CreateNav());
        vm.Email = "user@example.com";
        vm.Password = "wrong";
        await vm.LoginCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_EmptyEmail_ShowsValidationError()
    {
        var auth = new Mock<IAuthService>();
        var vm = new LoginViewModel(auth.Object, CreateNav());
        vm.Email = "";
        vm.Password = "somepass";
        await vm.LoginCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("E-mail é obrigatório");
        auth.Verify(a => a.LoginAsync(It.IsAny<LoginRequest>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ShowsValidationError()
    {
        var auth = new Mock<IAuthService>();
        var vm = new LoginViewModel(auth.Object, CreateNav());
        vm.Email = "user@example.com";
        vm.Password = "";
        await vm.LoginCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Senha é obrigatória");
        auth.Verify(a => a.LoginAsync(It.IsAny<LoginRequest>()), Times.Never);
    }

}

public class RegisterViewModelTests
{
    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
        }
    }

    private static NavigationManager CreateNav() => new TestNavigationManager();

    [Fact]
    public async Task RegisterAsync_PasswordMismatch_ShowsError()
    {
        var auth = new Mock<IAuthService>();
        var vm = new RegisterViewModel(auth.Object, CreateNav());
        vm.Password = "Senha123!";
        vm.ConfirmPassword = "OutraSenha";
        await vm.RegisterCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Senhas não conferem");
        auth.Verify(a => a.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_Success_Navigates()
    {
        var auth = new Mock<IAuthService>();
        auth
            .Setup(a => a.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Result.Ok());

        var vm = new RegisterViewModel(auth.Object, CreateNav());
        vm.FullName = "Test";
        vm.Email = "test@test.com";
        vm.Cpf = "52998224725";
        vm.Password = "Senha123!";
        vm.ConfirmPassword = "Senha123!";
        await vm.RegisterCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
    }
}
