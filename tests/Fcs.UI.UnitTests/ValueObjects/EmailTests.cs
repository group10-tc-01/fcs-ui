using Fcs.UI.Domain.Exceptions;
using Fcs.UI.Domain.ValueObjects;
using FluentAssertions;

namespace Fcs.UI.UnitTests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test@domain.com.br")]
    [InlineData("name.surname@company.co")]
    public void Parse_ValidEmail_ReturnsEmail(string raw)
    {
        var email = Email.Parse(raw);
        email.Value.Should().Be(raw.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanemail")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public void Parse_InvalidEmail_ThrowsDomainException(string raw)
    {
        Action act = () => Email.Parse(raw);
        act.Should().Throw<DomainException>().WithMessage("E-mail inválido");
    }

    [Fact]
    public void Parse_TrimsAndLowercases()
    {
        var email = Email.Parse("  User@Example.COM  ");
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void TryParse_ValidEmail_ReturnsTrue()
    {
        var success = Email.TryParse("user@example.com", out var email);
        success.Should().BeTrue();
        email!.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void TryParse_InvalidEmail_ReturnsFalse()
    {
        var success = Email.TryParse("invalid", out var email);
        success.Should().BeFalse();
        email.Should().BeNull();
    }

    [Fact]
    public void ImplicitConversion_StringToEmail_Valid()
    {
        Email email = "user@example.com";
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void ImplicitConversion_EmailToString_ReturnsValue()
    {
        var email = Email.Parse("user@example.com");
        string value = email;
        value.Should().Be("user@example.com");
    }
}
