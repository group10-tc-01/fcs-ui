using Fcs.UI.Domain.Enums;
using Fcs.UI.Domain.Exceptions;
using Fcs.UI.Domain.ValueObjects;

namespace Fcs.UI.Domain.Entities;

public sealed class Campaign
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Goal { get; private set; }
    public Money Raised { get; private set; }
    public CampaignStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime EndDate { get; private set; }

    public Campaign(string name, string description, Money goal, DateTime endDate)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Goal = goal;
        Raised = Money.Zero();
        Status = CampaignStatus.Active;
        CreatedAt = DateTime.UtcNow;
        EndDate = endDate;
    }

    public void AddDonation(Money amount)
    {
        if (Status != CampaignStatus.Active)
            throw new DomainException("Only active campaigns accept donations");
        if (DateTime.UtcNow > EndDate)
            throw new DomainException("Campaign has ended");

        Raised += amount;
        if (Raised >= Goal)
            Status = CampaignStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == CampaignStatus.Completed)
            throw new DomainException("Cannot cancel a completed campaign");
        Status = CampaignStatus.Cancelled;
    }
}
