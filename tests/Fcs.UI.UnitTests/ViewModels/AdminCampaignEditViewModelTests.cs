using Fcs.UI.Application.Commands;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Queries;
using Fcs.UI.Application.ViewModels;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Components;
using Moq;

namespace Fcs.UI.UnitTests.ViewModels;

public class AdminCampaignEditViewModelTests
{
    private sealed class TestNavigationManager : NavigationManager
    {
        public string? NavigatedTo { get; private set; }

        public TestNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            NavigatedTo = uri;
        }
    }

    private static AdminCampaignEditViewModel CreateVm(IMediator mediator, NavigationManager nav)
        => new(mediator, nav);

    [Fact]
    public async Task LoadAsync_ExistingCampaign_PopulatesFields()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        var campaignId = Guid.NewGuid();
        var campaigns = new List<CampaignDto>
        {
            new(campaignId, "Test Campaign", "A description",
                new DateTime(2026, 1, 1), new DateTime(2026, 6, 1), 5000m,
                "Active", 1000m, Guid.NewGuid(), DateTime.UtcNow, null)
        };
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<CampaignDto>>(campaigns));

        var vm = CreateVm(mediator.Object, nav);
        await vm.LoadCommand.ExecuteAsync(campaignId);

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
        vm.Title.Should().Be("Test Campaign");
        vm.Description.Should().Be("A description");
        vm.StartDate.Should().Be(new DateTime(2026, 1, 1));
        vm.EndDate.Should().Be(new DateTime(2026, 6, 1));
        vm.FinancialGoal.Should().Be(5000m);
        vm.IsEditing.Should().BeTrue();
        vm.TitleText.Should().Be("Editar Campanha");
    }

    [Fact]
    public async Task LoadAsync_CampaignNotFound_SetsError()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<CampaignDto>>([]));

        var vm = CreateVm(mediator.Object, nav);
        await vm.LoadCommand.ExecuteAsync(Guid.NewGuid());

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().Be("Campanha não encontrada");
    }

    [Fact]
    public async Task LoadAsync_QueryFails_SetsError()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyList<CampaignDto>>("Query error"));

        var vm = CreateVm(mediator.Object, nav);
        await vm.LoadCommand.ExecuteAsync(Guid.NewGuid());

        vm.ErrorMessage.Should().Be("Query error");
    }

    [Fact]
    public async Task LoadAsync_Exception_CapturesErrorMessage()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var vm = CreateVm(mediator.Object, nav);
        await vm.LoadCommand.ExecuteAsync(Guid.NewGuid());

        vm.ErrorMessage.Should().Be("DB error");
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_NewCampaign_SendsCreateCommandAndNavigates()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        mediator
            .Setup(m => m.Send(It.IsAny<CreateCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CampaignDto(Guid.NewGuid(), "", "", default, default, 0, "", 0, Guid.Empty, default, null)));

        var vm = CreateVm(mediator.Object, nav);
        vm.Title = "New Campaign";
        vm.FinancialGoal = 1000m;
        vm.EndDate = DateTime.Today.AddMonths(2);
        await vm.SaveCommand.ExecuteAsync(null);

        vm.IsSubmitting.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
        nav.NavigatedTo.Should().Be("/admin/campaigns");
        mediator.Verify(m => m.Send(It.IsAny<CreateCampaignCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(m => m.Send(It.IsAny<UpdateCampaignCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_EditCampaign_SendsUpdateCommand()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        var campaignId = Guid.NewGuid();
        var campaigns = new List<CampaignDto>
        {
            new(campaignId, "Old Title", "Old desc",
                DateTime.Today, DateTime.Today.AddMonths(1), 500m,
                "Active", 0m, Guid.NewGuid(), DateTime.UtcNow, null)
        };
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<CampaignDto>>(campaigns));
        mediator
            .Setup(m => m.Send(It.IsAny<UpdateCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CampaignDto(campaignId, "", "", default, default, 0, "", 0, Guid.Empty, default, null)));

        var vm = CreateVm(mediator.Object, nav);
        await vm.LoadCommand.ExecuteAsync(campaignId);
        vm.Title = "Updated Title";
        await vm.SaveCommand.ExecuteAsync(null);

        vm.IsSubmitting.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
        nav.NavigatedTo.Should().Be("/admin/campaigns");
        mediator.Verify(m => m.Send(It.IsAny<UpdateCampaignCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_EmptyTitle_ShowsValidationError()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        var vm = CreateVm(mediator.Object, nav);

        vm.Title = "";
        vm.FinancialGoal = 100m;
        vm.EndDate = DateTime.Today.AddMonths(1);
        await vm.SaveCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("O título é obrigatório");
        mediator.Verify(m => m.Send(It.IsAny<CreateCampaignCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_ZeroFinancialGoal_ShowsValidationError()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        var vm = CreateVm(mediator.Object, nav);

        vm.Title = "Campaign";
        vm.FinancialGoal = 0;
        vm.EndDate = DateTime.Today.AddMonths(1);
        await vm.SaveCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("A meta financeira deve ser maior que zero");
    }

    [Fact]
    public async Task SaveAsync_EndDateBeforeStart_ShowsValidationError()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        var vm = CreateVm(mediator.Object, nav);

        vm.Title = "Campaign";
        vm.FinancialGoal = 1000m;
        vm.EndDate = DateTime.Today.AddDays(-1);
        await vm.SaveCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("A data de término deve ser posterior à data de início");
    }

    [Fact]
    public async Task SaveAsync_CreateFails_ShowsError()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        mediator
            .Setup(m => m.Send(It.IsAny<CreateCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<CampaignDto>("Create failed"));

        var vm = CreateVm(mediator.Object, nav);
        vm.Title = "Campaign";
        vm.FinancialGoal = 500m;
        vm.EndDate = DateTime.Today.AddMonths(1);
        await vm.SaveCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Create failed");
        vm.IsSubmitting.Should().BeFalse();
        nav.NavigatedTo.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_Exception_CapturesErrorMessage()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        mediator
            .Setup(m => m.Send(It.IsAny<CreateCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Server error"));

        var vm = CreateVm(mediator.Object, nav);
        vm.Title = "Campaign";
        vm.FinancialGoal = 500m;
        vm.EndDate = DateTime.Today.AddMonths(1);
        await vm.SaveCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Server error");
        vm.IsSubmitting.Should().BeFalse();
    }

    [Fact]
    public void IsEditing_FalseByDefault()
    {
        var mediator = new Mock<IMediator>();
        var nav = new TestNavigationManager();
        var vm = CreateVm(mediator.Object, nav);

        vm.IsEditing.Should().BeFalse();
        vm.TitleText.Should().Be("Nova Campanha");
    }
}
