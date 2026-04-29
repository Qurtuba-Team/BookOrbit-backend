namespace BookOrbit.Domain.UnitTests.PointTransactions.ValueObjects;

using BookOrbit.Domain.PointTransactions.ValueObjects;
using FluentAssertions;
using Xunit;

public class PointTests
{
    [Fact]
    public void Create_WithNullValue_ReturnsRequiredPointError()
    {
        // Act
        var result = Point.Create(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointErrors.RequiredPoint);
    }

    [Theory]
    [InlineData(Point.MinValue)]
    [InlineData(5)]
    public void Create_WithValidValue_ReturnsSuccess(int value)
    {
        // Act
        var result = Point.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithInvalidValue_ReturnsInvalidPointError(int value)
    {
        // Act
        var result = Point.Create(value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointErrors.InvalidPoint);
    }
}
