using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using FluentResults;
using MediatR;

namespace Fcs.UI.Application.Commands;

public sealed record CreateCampaignCommand(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal) : IRequest<Result<CampaignDto>>;

public sealed class CreateCampaignHandler(ICampaignRepository repo)
    : IRequestHandler<CreateCampaignCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(
        CreateCampaignCommand cmd, CancellationToken ct)
    {
        var request = new CreateCampaignRequest(
            cmd.Title, cmd.Description, cmd.StartDate, cmd.EndDate, cmd.FinancialGoal);

        var result = await repo.CreateAsync(request);
        return result is not null
            ? Result.Ok(result)
            : Result.Fail("Falha ao criar campanha");
    }
}
