using Fcs.UI.Domain.Entities;
using Fcs.UI.Domain.Enums;
using Fcs.UI.Domain.Exceptions;
using Fcs.UI.Domain.ValueObjects;
using FluentAssertions;

namespace Fcs.UI.UnitTests.Entities;

public class CampaignTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var goal = new Money(10000);
        var endDate = DateTime.UtcNow.AddDays(30);
        var campaign = new Campaign("Test", "Description", goal, endDate);

        campaign.Id.Should().NotBeEmpty();
        campaign.Name.Should().Be("Test");
        campaign.Description.Should().Be("Description");
        campaign.Goal.Should().Be(goal);
        campaign.Raised.Should().Be(Money.Zero());
        campaign.Status.Should().Be(CampaignStatus.Active);
        campaign.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void AddDonation_UnderGoal_IncreasesRaised()
    {
        var campaign = new Campaign("Test", "", new Money(1000), DateTime.UtcNow.AddDays(30));
        campaign.AddDonation(new Money(300));
        campaign.Raised.Amount.Should().Be(300);
        campaign.Status.Should().Be(CampaignStatus.Active);
    }

    [Fact]
    public void AddDonation_ReachesGoal_CompletesCampaign()
    {
        var campaign = new Campaign("Test", "", new Money(1000), DateTime.UtcNow.AddDays(30));
        campaign.AddDonation(new Money(1000));
        campaign.Raised.Amount.Should().Be(1000);
        campaign.Status.Should().Be(CampaignStatus.Completed);
    }

    [Fact]
    public void AddDonation_ExceedsGoal_CompletesAndTracksExcess()
    {
        var campaign = new Campaign("Test", "", new Money(1000), DateTime.UtcNow.AddDays(30));
        campaign.AddDonation(new Money(1500));
        campaign.Raised.Amount.Should().Be(1500);
        campaign.Status.Should().Be(CampaignStatus.Completed);
    }

    [Fact]
    public void AddDonation_OnCompletedCampaign_Throws()
    {
        var campaign = new Campaign("Test", "", new Money(100), DateTime.UtcNow.AddDays(30));
        campaign.AddDonation(new Money(100));

        Action act = () => campaign.AddDonation(new Money(50));
        act.Should().Throw<DomainException>().WithMessage("Only active campaigns accept donations");
    }

    [Fact]
    public void AddDonation_OnCancelledCampaign_Throws()
    {
        var campaign = new Campaign("Test", "", new Money(1000), DateTime.UtcNow.AddDays(30));
        campaign.Cancel();

        Action act = () => campaign.AddDonation(new Money(50));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_ActiveCampaign_SetsCancelled()
    {
        var campaign = new Campaign("Test", "", new Money(1000), DateTime.UtcNow.AddDays(30));
        campaign.Cancel();
        campaign.Status.Should().Be(CampaignStatus.Cancelled);
    }

    [Fact]
    public void Cancel_CompletedCampaign_Throws()
    {
        var campaign = new Campaign("Test", "", new Money(100), DateTime.UtcNow.AddDays(30));
        campaign.AddDonation(new Money(100));

        Action act = () => campaign.Cancel();
        act.Should().Throw<DomainException>().WithMessage("Cannot cancel a completed campaign");
    }
}
