using Fcs.UI.Application.Commands;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Domain.Entities;
using Fcs.UI.Domain.Events;
using FluentAssertions;
using Moq;

namespace Fcs.UI.UnitTests.UseCases;

public class SubmitDonationHandlerTests
{
    [Fact]
    public async Task Handle_ValidDonation_ReturnsSuccess()
    {
        var repo = new Mock<IDonationRepository>();
        var events = new Mock<IEventStore>();
        var handler = new SubmitDonationHandler(repo.Object, events.Object);
        var cmd = new SubmitDonationCommand(Guid.NewGuid(), 100m, "52998224725");

        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.DonationId.Should().NotBeEmpty();
        result.Value.Status.Should().Be("Pending");

        repo.Verify(r => r.AddAsync(It.Is<Donation>(d =>
            d.CampaignId == cmd.CampaignId &&
            d.Amount == cmd.Amount &&
            d.DonorCpf == cmd.DonorCpf)), Times.Once);

        events.Verify(e => e.AppendAsync(It.Is<DonationMadeEvent>(ev =>
            ev.CampaignId == cmd.CampaignId &&
            ev.Amount == cmd.Amount), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ZeroOrNegativeAmount_StillProcesses()
    {
        var repo = new Mock<IDonationRepository>();
        var events = new Mock<IEventStore>();
        var handler = new SubmitDonationHandler(repo.Object, events.Object);
        var cmd = new SubmitDonationCommand(Guid.NewGuid(), 0m, "52998224725");

        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.AddAsync(It.IsAny<Donation>()), Times.Once);
    }
}
