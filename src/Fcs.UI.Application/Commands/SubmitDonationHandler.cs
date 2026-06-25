using Fcs.UI.Application.Interfaces;
using Fcs.UI.Domain.Entities;
using Fcs.UI.Domain.Events;
using FluentResults;
using MediatR;

namespace Fcs.UI.Application.Commands;

public sealed class SubmitDonationHandler(
    IDonationRepository repo,
    IEventStore events)
    : IRequestHandler<SubmitDonationCommand, Result<DonationResponse>>
{
    public async Task<Result<DonationResponse>> Handle(
        SubmitDonationCommand cmd, CancellationToken ct)
    {
        var donation = new Donation(cmd.CampaignId, cmd.Amount, cmd.DonorCpf);

        await repo.AddAsync(donation);
        await events.AppendAsync(new DonationMadeEvent(
            donation.Id, cmd.CampaignId, cmd.Amount, DateTime.UtcNow), donation.Id);

        return Result.Ok(new DonationResponse(donation.Id, donation.Status.ToString()));
    }
}
