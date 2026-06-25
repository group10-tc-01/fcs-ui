using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.DTOs;

[ExcludeFromCodeCoverage]
public sealed record DonationDto(
    Guid Id,
    Guid CampaignId,
    Guid DonorId,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    string? FailureReason,
    string? CampaignTitle = null);
