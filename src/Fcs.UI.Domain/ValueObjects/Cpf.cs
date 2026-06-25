using Fcs.UI.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Fcs.UI.Domain.ValueObjects;

public sealed record Cpf
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Cpf Parse(string raw)
    {
        var digits = Regex.Replace(raw, @"\D", "");
        if (digits.Length != 11 || !ValidarDigitos(digits))
            throw new DomainException("CPF inválido");
        return new Cpf(digits);
    }

    public static bool TryParse(string raw, out Cpf? result)
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

    public static implicit operator string(Cpf cpf) => cpf.Value;

    public static implicit operator Cpf(string raw) => Parse(raw);

    public override string ToString() =>
        Convert.ToUInt64(Value).ToString(@"000\.000\.000\-00");

    private static bool ValidarDigitos(string digits)
    {
        if (digits.Distinct().Count() == 1) return false;

        int[] mult1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] mult2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        var sum = digits.Take(9).Select((d, i) => (d - '0') * mult1[i]).Sum();
        var resto = sum % 11;
        var dig1 = resto < 2 ? 0 : 11 - resto;
        if (dig1 != digits[9] - '0') return false;

        sum = digits.Take(10).Select((d, i) => (d - '0') * mult2[i]).Sum();
        resto = sum % 11;
        var dig2 = resto < 2 ? 0 : 11 - resto;
        return dig2 == digits[10] - '0';
    }
}
