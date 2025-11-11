using CRM.Domain.Abstractions;

namespace CRM.Domain.Common.ValueObjects;

public sealed class DateRange : ValueObject
{
    private DateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public DateTime Start { get; }
    public DateTime End { get; }

    public static DateRange Create(DateTime start, DateTime end)
    {
        if (end < start)
        {
            throw new ArgumentException("Bitiş tarihi başlangıç tarihinden önce olamaz.", nameof(end));
        }

        return new DateRange(start, end);
    }

    public bool Contains(DateTime date) => date >= Start && date <= End;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}

