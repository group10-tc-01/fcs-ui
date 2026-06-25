using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using FluentResults;
using MediatR;

namespace Fcs.UI.Application.Commands;

public sealed record CompleteCampaignCommand(Guid Id) : IRequest<Result<CampaignDto>>;

public sealed class CompleteCampaignHandler(ICampaignRepository repo)
    : IRequestHandler<CompleteCampaignCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(
        CompleteCampaignCommand cmd, CancellationToken ct)
    {
        var result = await repo.CompleteAsync(cmd.Id);
        return result is not null
            ? Result.Ok(result)
            : Result.Fail("Falha ao concluir campanha");
    }
}
