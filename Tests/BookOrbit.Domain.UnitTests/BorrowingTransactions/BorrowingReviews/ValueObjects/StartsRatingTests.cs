namespace BookOrbit.Domain.UnitTests.BorrowingTransactions.BorrowingReviews.ValueObjects;

using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;
using FluentAssertions;
using Xunit;

public class StartsRatingTests
{
    [Fact]
    public void Create_WithNullRating_ReturnsRatingRequiredError()
    {
        // Act
        var result = StartsRating.Create(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.RatingRequired);
    }

    [Theory]
    [InlineData(StartsRating.MinRating)]
    [InlineData(StartsRating.MaxRating)]
    public void Create_WithValidRating_ReturnsSuccess(int rating)
    {
        // Act
        var result = StartsRating.Create(rating);

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
        var result = StartsRating.Create(rating);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.InvalidRating);
    }
}
