using Fcs.UI.Application.DTOs;
using FluentResults;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.Queries;

[ExcludeFromCodeCoverage]
public sealed record GetAdminCampaignsQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<IReadOnlyList<CampaignDto>>>;
