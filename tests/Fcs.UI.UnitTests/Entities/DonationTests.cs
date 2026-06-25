using Fcs.UI.Domain.Entities;
using Fcs.UI.Domain.Enums;
using FluentAssertions;

namespace Fcs.UI.UnitTests.Entities;

public class DonationTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var campaignId = Guid.NewGuid();
        var donation = new Donation(campaignId, 100m, "52998224725");

        donation.Id.Should().NotBeEmpty();
        donation.CampaignId.Should().Be(campaignId);
        donation.Amount.Should().Be(100m);
        donation.DonorCpf.Should().Be("52998224725");
        donation.Status.Should().Be(DonationStatus.Pending);
        donation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Confirm_PendingDonation_SetsConfirmed()
    {
        var donation = new Donation(Guid.NewGuid(), 100m, "52998224725");
        donation.Confirm();
        donation.Status.Should().Be(DonationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_Throws()
    {
        var donation = new Donation(Guid.NewGuid(), 100m, "52998224725");
        donation.Confirm();

        Action act = () => donation.Confirm();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only pending donations can be confirmed");
    }

    [Fact]
    public void Confirm_FailedDonation_Throws()
    {
        var donation = new Donation(Guid.NewGuid(), 100m, "52998224725");
        donation.Fail();

        Action act = () => donation.Confirm();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fail_PendingDonation_SetsFailed()
    {
        var donation = new Donation(Guid.NewGuid(), 100m, "52998224725");
        donation.Fail();
        donation.Status.Should().Be(DonationStatus.Failed);
    }

    [Fact]
    public void Fail_AlreadyFailed_Throws()
    {
        var donation = new Donation(Guid.NewGuid(), 100m, "52998224725");
        donation.Fail();

        Action act = () => donation.Fail();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only pending donations can fail");
    }
}
