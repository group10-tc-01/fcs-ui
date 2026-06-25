using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using FluentResults;
using MediatR;

namespace Fcs.UI.Application.Queries;

public sealed class GetAdminCampaignsHandler(ICampaignRepository repo)
    : IRequestHandler<GetAdminCampaignsQuery, Result<IReadOnlyList<CampaignDto>>>
{
    public async Task<Result<IReadOnlyList<CampaignDto>>> Handle(
        GetAdminCampaignsQuery query, CancellationToken ct)
    {
        try
        {
            var campaigns = await repo.GetAllAsync(query.Page, query.PageSize);
            return Result.Ok(campaigns);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
