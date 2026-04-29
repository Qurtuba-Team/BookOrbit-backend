namespace BookOrbit.Domain.UnitTests.BorrowingTransactions.BorrowingReviews;

using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;
using FluentAssertions;
using Xunit;

public class BorrowingReviewTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var reviewedId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var rating = StarsRating.Create(4).Value;
        var description = "Great experience";

        // Act
        var result = BorrowingReview.Create(id, reviewerId, reviewedId, transactionId, description, rating);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.ReviewerStudentId.Should().Be(reviewerId);
        result.Value.ReviewedStudentId.Should().Be(reviewedId);
        result.Value.BorrowingTransactionId.Should().Be(transactionId);
        result.Value.Description.Should().Be(description);
        result.Value.Rating.Should().Be(rating);
    }

    [Fact]
    public void Create_WithWhitespaceDescription_SetsDescriptionToNull()
    {
        // Arrange
        var rating = StarsRating.Create(3).Value;

        // Act
        var result = BorrowingReview.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "   ", rating);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var rating = StarsRating.Create(3).Value;

        // Act
        var result = BorrowingReview.Create(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, rating);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.IdRequired);
    }

    [Fact]
    public void Create_WithEmptyReviewerStudentId_ReturnsReviewerStudentIdRequiredError()
    {
        // Arrange
        var rating = StarsRating.Create(3).Value;

        // Act
        var result = BorrowingReview.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), null, rating);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.ReviewerStudentIdRequired);
    }

    [Fact]
    public void Create_WithEmptyReviewedStudentId_ReturnsReviewedStudentIdRequiredError()
    {
        // Arrange
        var rating = StarsRating.Create(3).Value;

        // Act
        var result = BorrowingReview.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), null, rating);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.ReviewedStudentIdRequired);
    }

    [Fact]
    public void Create_WithEmptyBorrowingTransactionId_ReturnsBorrowingTransactionIdRequiredError()
    {
        // Arrange
        var rating = StarsRating.Create(3).Value;

        // Act
        var result = BorrowingReview.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, null, rating);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.BorrowingTransactionIdRequired);
    }

    [Fact]
    public void Create_WithNullRating_ReturnsRatingRequiredError()
    {
        // Act
        var result = BorrowingReview.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingReviewErrors.RatingRequired);
    }
}
