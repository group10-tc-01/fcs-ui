using Fcs.UI.Application.Commands;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Queries;
using Fcs.UI.Application.ViewModels;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;

namespace Fcs.UI.UnitTests.ViewModels;

public class AdminCampaignListViewModelTests
{
    [Fact]
    public async Task LoadAsync_Success_PopulatesCampaigns()
    {
        var mediator = new Mock<IMediator>();
        var campaigns = new List<CampaignDto>
        {
            new(Guid.NewGuid(), "Camp1", "", default, default, 0, "", 0, Guid.Empty, default, null),
            new(Guid.NewGuid(), "Camp2", "", default, default, 0, "", 0, Guid.Empty, default, null),
        };
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<CampaignDto>>(campaigns));

        var vm = new AdminCampaignListViewModel(mediator.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
        vm.Campaigns.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_Failure_ShowsError()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyList<CampaignDto>>("Failed"));

        var vm = new AdminCampaignListViewModel(mediator.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Failed");
        vm.Campaigns.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_Exception_ShowsError()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var vm = new AdminCampaignListViewModel(mediator.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("DB error");
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteAsync_Success_ReloadsCampaigns()
    {
        var mediator = new Mock<IMediator>();
        var campaignId = Guid.NewGuid();
        mediator
            .Setup(m => m.Send(It.IsAny<CompleteCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CampaignDto(campaignId, "", "", default, default, 0, "", 0, Guid.Empty, default, null)));
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<CampaignDto>>([]));

        var vm = new AdminCampaignListViewModel(mediator.Object);
        await vm.CompleteCommand.ExecuteAsync(campaignId);

        vm.ErrorMessage.Should().BeNull();
        mediator.Verify(m => m.Send(It.IsAny<CompleteCampaignCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_Failure_ShowsError()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<CompleteCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<CampaignDto>("Complete failed"));

        var vm = new AdminCampaignListViewModel(mediator.Object);
        await vm.CompleteCommand.ExecuteAsync(Guid.NewGuid());

        vm.ErrorMessage.Should().Be("Complete failed");
    }

    [Fact]
    public async Task CancelAsync_Success_ReloadsCampaigns()
    {
        var mediator = new Mock<IMediator>();
        var campaignId = Guid.NewGuid();
        mediator
            .Setup(m => m.Send(It.IsAny<CancelCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CampaignDto(campaignId, "", "", default, default, 0, "", 0, Guid.Empty, default, null)));
        mediator
            .Setup(m => m.Send(It.IsAny<GetAdminCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<CampaignDto>>([]));

        var vm = new AdminCampaignListViewModel(mediator.Object);
        await vm.CancelCommand.ExecuteAsync(campaignId);

        vm.ErrorMessage.Should().BeNull();
        mediator.Verify(m => m.Send(It.IsAny<CancelCampaignCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_Failure_ShowsError()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<CancelCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<CampaignDto>("Cancel failed"));

        var vm = new AdminCampaignListViewModel(mediator.Object);
        await vm.CancelCommand.ExecuteAsync(Guid.NewGuid());

        vm.ErrorMessage.Should().Be("Cancel failed");
    }
}
