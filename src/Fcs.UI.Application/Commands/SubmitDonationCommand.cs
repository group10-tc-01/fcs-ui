using FluentResults;
using MediatR;

namespace Fcs.UI.Application.Commands;

public sealed record SubmitDonationCommand(
    Guid CampaignId,
    decimal Amount,
    string DonorCpf
) : IRequest<Result<DonationResponse>>;

public sealed record DonationResponse(Guid DonationId, string Status);
