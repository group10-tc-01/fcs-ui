using Fcs.UI.Application.Commands;
using FluentValidation;

namespace Fcs.UI.Application.Validators;

public sealed class SubmitDonationValidator : AbstractValidator<SubmitDonationCommand>
{
    public SubmitDonationValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.DonorCpf).SetValidator(new CpfValidator());
        RuleFor(x => x.CampaignId).NotEmpty();
    }
}
