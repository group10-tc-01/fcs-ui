using Fcs.UI.Domain.Exceptions;
using System.Globalization;

namespace Fcs.UI.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new DomainException("Valor não pode ser negativo");
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency = "BRL") => new(0, currency);

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Moedas diferentes não podem ser somadas");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Moedas diferentes não podem ser subtraídas");
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static bool operator >=(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Moedas diferentes não podem ser comparadas");
        return a.Amount >= b.Amount;
    }

    public static bool operator <=(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Moedas diferentes não podem ser comparadas");
        return a.Amount <= b.Amount;
    }

    public override string ToString() => $"{Amount.ToString("F2", CultureInfo.InvariantCulture)} {Currency}";
}
