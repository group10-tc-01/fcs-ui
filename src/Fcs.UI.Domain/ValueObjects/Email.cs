using Fcs.UI.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Fcs.UI.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex Pattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException("E-mail inválido");
        var trimmed = raw.Trim().ToLowerInvariant();
        if (!Pattern.IsMatch(trimmed))
            throw new DomainException("E-mail inválido");
        return new Email(trimmed);
    }

    public static bool TryParse(string raw, out Email? result)
    {
        try
        {
            result = Parse(raw);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public static implicit operator string(Email email) => email.Value;

    public static implicit operator Email(string raw) => Parse(raw);

    public override string ToString() => Value;
}
