using Fcs.UI.Domain.Exceptions;
using Fcs.UI.Domain.ValueObjects;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace Fcs.UI.UnitTests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Parse_ValidCpf_ReturnsCpf(string raw)
    {
        var cpf = Cpf.Parse(raw);
        cpf.Value.Should().Be("52998224725");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("abc.def.ghi-jk")]
    public void Parse_InvalidCpf_ThrowsDomainException(string raw)
    {
        Action act = () => Cpf.Parse(raw);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TryParse_ValidCpf_ReturnsTrue()
    {
        var success = Cpf.TryParse("529.982.247-25", out var cpf);
        success.Should().BeTrue();
        cpf!.Value.Should().Be("52998224725");
    }

    [Fact]
    public void TryParse_InvalidCpf_ReturnsFalse()
    {
        var success = Cpf.TryParse("00000000000", out var cpf);
        success.Should().BeFalse();
        cpf.Should().BeNull();
    }

    [Fact]
    public void ToString_ReturnsFormattedCpf()
    {
        var cpf = Cpf.Parse("52998224725");
        cpf.ToString().Should().Be("529.982.247-25");
    }

    [Fact]
    public void ImplicitConversion_StringToCpf_Valid()
    {
        Cpf cpf = "529.982.247-25";
        cpf.Value.Should().Be("52998224725");
    }

    [Fact]
    public void ImplicitConversion_CpfToString_ReturnsValue()
    {
        var cpf = Cpf.Parse("52998224725");
        string value = cpf;
        value.Should().Be("52998224725");
    }

    [Property]
    public void TryParse_AnyString_DoesNotThrow(NonEmptyString raw)
    {
        var exception = Record.Exception(() => Cpf.TryParse(raw.Get, out _));
        Assert.Null(exception);
    }
}
