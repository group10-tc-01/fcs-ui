using Fcs.UI.Domain.Exceptions;
using Fcs.UI.Domain.ValueObjects;
using FluentAssertions;

namespace Fcs.UI.UnitTests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_DefaultCurrency_IsBRL()
    {
        var money = new Money(100);
        money.Currency.Should().Be("BRL");
        money.Amount.Should().Be(100m);
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsDomainException()
    {
        Action act = () => new Money(-1);
        act.Should().Throw<DomainException>().WithMessage("Valor não pode ser negativo");
    }

    [Fact]
    public void Zero_ReturnsZeroMoney()
    {
        var zero = Money.Zero();
        zero.Amount.Should().Be(0);
        zero.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Addition_SameCurrency_ReturnsSum()
    {
        var a = new Money(100);
        var b = new Money(50);
        var sum = a + b;

        sum.Amount.Should().Be(150);
        sum.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Addition_DifferentCurrency_ThrowsDomainException()
    {
        var a = new Money(100, "BRL");
        var b = new Money(50, "USD");

        Action act = () => _ = a + b;
        act.Should().Throw<DomainException>().WithMessage("Moedas diferentes não podem ser somadas");
    }

    [Fact]
    public void Subtraction_SameCurrency_ReturnsDifference()
    {
        var a = new Money(100);
        var b = new Money(30);
        var diff = a - b;

        diff.Amount.Should().Be(70);
        diff.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Subtraction_DifferentCurrency_ThrowsDomainException()
    {
        var a = new Money(100, "BRL");
        var b = new Money(30, "USD");

        Action act = () => _ = a - b;
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void GreaterThanOrEqual_SameCurrency_ComparesCorrectly()
    {
        var a = new Money(100);
        var b = new Money(50);
        var c = new Money(100);

        (a >= b).Should().BeTrue();
        (a >= c).Should().BeTrue();
        (b >= a).Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOrEqual_DifferentCurrency_Throws()
    {
        var a = new Money(100, "BRL");
        var b = new Money(50, "USD");

        Action act = () => _ = a >= b;
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        var money = new Money(1234.56m);
        var result = money.ToString();
        result.Should().Contain("1234");
        result.Should().Contain("56");
        result.Should().Contain("BRL");
    }
}
