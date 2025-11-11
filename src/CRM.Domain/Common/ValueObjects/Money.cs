using CRM.Domain.Abstractions;

namespace CRM.Domain.Common.ValueObjects;

public sealed class Money : ValueObject
{
    private Money(decimal amount, string currency)
    {
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Money From(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ArgumentException("Para birimi ISO 4217 formatında olmalıdır.", nameof(currency));
        }

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    private void EnsureSameCurrency(Money other)
    {
        if (!Currency.Equals(other.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Farklı para birimleri arasında işlem yapılamaz.");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

