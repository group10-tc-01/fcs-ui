using Fcs.UI.Domain.ValueObjects;
using FluentValidation;

namespace Fcs.UI.Application.Validators;

public sealed class CpfValidator : AbstractValidator<string>
{
    public CpfValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .Must(cpf => Cpf.TryParse(cpf!, out _))
            .WithMessage("CPF inválido");
    }
}
