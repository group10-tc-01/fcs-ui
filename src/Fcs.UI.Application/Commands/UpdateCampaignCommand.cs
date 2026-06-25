using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using FluentResults;
using MediatR;

namespace Fcs.UI.Application.Commands;

public sealed record UpdateCampaignCommand(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal) : IRequest<Result<CampaignDto>>;

public sealed class UpdateCampaignHandler(ICampaignRepository repo)
    : IRequestHandler<UpdateCampaignCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(
        UpdateCampaignCommand cmd, CancellationToken ct)
    {
        var request = new UpdateCampaignRequest(
            cmd.Title, cmd.Description, cmd.StartDate, cmd.EndDate, cmd.FinancialGoal);

        var result = await repo.UpdateAsync(cmd.Id, request);
        return result is not null
            ? Result.Ok(result)
            : Result.Fail("Falha ao atualizar campanha");
    }
}
