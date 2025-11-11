using CRM.Domain.Abstractions;

namespace CRM.Domain.Common.ValueObjects;

public sealed class Measurement : ValueObject
{
    private Measurement(decimal amount, string unit)
    {
        Amount = decimal.Round(amount, 4, MidpointRounding.AwayFromZero);
        Unit = unit;
    }

    public decimal Amount { get; }
    public string Unit { get; }

    public static Measurement CubicMeters(decimal amount) => Create(amount, "m3");

    public static Measurement Create(decimal amount, string unit)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Ölçüm değeri negatif olamaz.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new ArgumentException("Ölçüm birimi zorunludur.", nameof(unit));
        }

        return new Measurement(amount, unit.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Unit;
    }
}

