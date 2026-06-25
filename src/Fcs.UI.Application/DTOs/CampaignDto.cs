using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.DTOs;

[ExcludeFromCodeCoverage]
public sealed record CampaignDto(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal,
    string Status,
    decimal TotalAmountRaised,
    Guid CreatedByManagerId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
