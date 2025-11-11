using CRM.Domain.Common.ValueObjects;
using FluentAssertions;

namespace CRM.UnitTests.Common.ValueObjects;

public class MeasurementTests
{
    [Theory]
    [InlineData(12.34567, "m3", 12.3457)]
    [InlineData(0.987654, "M3", 0.9877)]
    public void Create_ShouldNormalizeAmountAndUnit(decimal amount, string unit, decimal expected)
    {
        var measurement = Measurement.Create(amount, unit);

        measurement.Amount.Should().Be(expected);
        measurement.Unit.Should().Be("M3");
    }

    [Fact]
    public void Create_ShouldThrow_WhenAmountIsNegative()
    {
        var act = () => Measurement.Create(-1, "M3");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameValues()
    {
        var left = Measurement.CubicMeters(10);
        var right = Measurement.Create(10, "m3");

        left.Should().Be(right);
    }
}

