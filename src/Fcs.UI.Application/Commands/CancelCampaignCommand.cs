using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using FluentResults;
using MediatR;

namespace Fcs.UI.Application.Commands;

public sealed record CancelCampaignCommand(Guid Id) : IRequest<Result<CampaignDto>>;

public sealed class CancelCampaignHandler(ICampaignRepository repo)
    : IRequestHandler<CancelCampaignCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(
        CancelCampaignCommand cmd, CancellationToken ct)
    {
        var result = await repo.CancelAsync(cmd.Id);
        return result is not null
            ? Result.Ok(result)
            : Result.Fail("Falha ao cancelar campanha");
    }
}
