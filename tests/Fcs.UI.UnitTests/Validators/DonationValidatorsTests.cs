using Fcs.UI.Application.Commands;
using Fcs.UI.Application.Validators;
using FluentAssertions;

namespace Fcs.UI.UnitTests.Validators;

public class CpfValidatorTests
{
    private readonly CpfValidator _sut = new();

    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void Validate_ValidCpf_Passes(string cpf)
    {
        var result = _sut.Validate(cpf);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("00000000000")]
    [InlineData("123")]
    public void Validate_InvalidCpf_Fails(string cpf)
    {
        var result = _sut.Validate(cpf);
        result.IsValid.Should().BeFalse();
    }
}

public class SubmitDonationValidatorTests
{
    private readonly SubmitDonationValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var cmd = new SubmitDonationCommand(Guid.NewGuid(), 100m, "52998224725");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ZeroAmount_Fails()
    {
        var cmd = new SubmitDonationCommand(Guid.NewGuid(), 0m, "52998224725");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidCpf_Fails()
    {
        var cmd = new SubmitDonationCommand(Guid.NewGuid(), 100m, "00000000000");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyCampaignId_Fails()
    {
        var cmd = new SubmitDonationCommand(Guid.Empty, 100m, "52998224725");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}
