using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.DTOs;

[ExcludeFromCodeCoverage]
public sealed record ActiveDonorCampaignDto(
    Guid Id,
    string Title,
    decimal FinancialGoal,
    decimal TotalAmountRaised,
    DateTime StartDate,
    DateTime EndDate);
