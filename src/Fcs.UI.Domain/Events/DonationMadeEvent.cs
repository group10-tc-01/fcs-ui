using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Domain.Events;

[ExcludeFromCodeCoverage]
public sealed record DonationMadeEvent(
    Guid DonationId,
    Guid CampaignId,
    decimal Amount,
    DateTime OccurredAt
) : IDomainEvent;
