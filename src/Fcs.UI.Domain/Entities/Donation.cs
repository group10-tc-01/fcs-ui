using Fcs.UI.Domain.Enums;

namespace Fcs.UI.Domain.Entities;

public sealed class Donation
{
    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public string DonorCpf { get; private set; }
    public decimal Amount { get; private set; }
    public DonationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Donation(Guid campaignId, decimal amount, string donorCpf)
    {
        Id = Guid.NewGuid();
        CampaignId = campaignId;
        Amount = amount;
        DonorCpf = donorCpf;
        Status = DonationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != DonationStatus.Pending)
            throw new InvalidOperationException("Only pending donations can be confirmed");
        Status = DonationStatus.Confirmed;
    }

    public void Fail()
    {
        if (Status != DonationStatus.Pending)
            throw new InvalidOperationException("Only pending donations can fail");
        Status = DonationStatus.Failed;
    }
}
