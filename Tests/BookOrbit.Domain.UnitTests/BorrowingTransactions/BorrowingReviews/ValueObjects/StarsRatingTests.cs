namespace BookOrbit.Domain.UnitTests.BorrowingTransactions.BorrowingReviews.ValueObjects;

using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;
using FluentAssertions;
using Xunit;

public class StarsRatingTests
{
    [Fact]
    public void Create_WithNullRating_ReturnsRatingRequiredError()
    {
        // Act
        var result = StarsRating.Create(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.RatingRequired);
    }

    [Theory]
    [InlineData(StarsRating.MinRating)]
    [InlineData(StarsRating.MaxRating)]
    public void Create_WithValidRating_ReturnsSuccess(int rating)
    {
        // Act
        var result = StarsRating.Create(rating);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Create_WithInvalidRating_ReturnsInvalidRatingError(int rating)
    {
        // Act
        var result = StarsRating.Create(rating);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.InvalidRating);
    }
}
