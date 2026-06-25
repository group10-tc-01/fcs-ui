using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.DTOs;

[ExcludeFromCodeCoverage]
public sealed record UpdateCampaignRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal);
