namespace BookOrbit.Domain.UnitTests.PointTransactions;

using BookOrbit.Domain.PointTransactions;
using BookOrbit.Domain.PointTransactions.Enums;
using FluentAssertions;
using Xunit;

public class PointTransactionTests
{
    [Fact]
    public void Create_WithGoodReviewReasonAndBorrowingReviewId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        // Act
        var result = PointTransaction.Create(id, studentId, reviewId, 5, PointTransactionReason.GoodReview);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Direction.Should().Be(PointTransactionDirection.Add);
        result.Value.BorrowingReviewId.Should().Be(reviewId);
    }

    [Fact]
    public void Create_WithBadReviewReasonAndBorrowingReviewId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        // Act
        var result = PointTransaction.Create(id, studentId, reviewId, 3, PointTransactionReason.BadReview);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Direction.Should().Be(PointTransactionDirection.Deduct);
        result.Value.BorrowingReviewId.Should().Be(reviewId);
    }

    [Fact]
    public void Create_WithNonReviewReasonAndNoBorrowingReviewId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        // Act
        var result = PointTransaction.Create(id, studentId, null, 2, PointTransactionReason.Borrowing);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Direction.Should().Be(PointTransactionDirection.Deduct);
        result.Value.BorrowingReviewId.Should().BeNull();
    }

    [Fact]
    public void Create_WithReviewReasonWithoutBorrowingReviewId_ReturnsReviewReasonShouldHaveBorrowingReviewIdError()
    {
        // Act
        var result = PointTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), null, 2, PointTransactionReason.GoodReview);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointTransactionErrors.ReviewReasonShouldHaveBorrowingReviewId);
    }

    [Fact]
    public void Create_WithNonReviewReasonAndBorrowingReviewId_ReturnsNonReviewReasonShouldNotHaveBorrowingReviewIdError()
    {
        // Act
        var result = PointTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2, PointTransactionReason.Borrowing);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointTransactionErrors.NonReviewReasonShouldNotHaveBorrowingReviewId);
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Act
        var result = PointTransaction.Create(Guid.Empty, Guid.NewGuid(), null, 2, PointTransactionReason.Borrowing);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointTransactionErrors.IdRequired);
    }

    [Fact]
    public void Create_WithEmptyStudentId_ReturnsStudentIdRequiredError()
    {
        // Act
        var result = PointTransaction.Create(Guid.NewGuid(), Guid.Empty, null, 2, PointTransactionReason.Borrowing);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointTransactionErrors.StudentIdRequired);
    }

    [Fact]
    public void Create_WithInvalidBorrowingReviewId_ReturnsInvalidBorrowingReviewIdError()
    {
        // Act
        var result = PointTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 2, PointTransactionReason.Borrowing);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointTransactionErrors.InvalidBorrowingReviewId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidPoints_ReturnsInvalidPointsError(int points)
    {
        // Act
        var result = PointTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), null, points, PointTransactionReason.Borrowing);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointTransactionErrors.InvalidPoints);
    }

    [Fact]
    public void Create_WithInvalidReason_ReturnsInvalidReasonError()
    {
        // Act
        var result = PointTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), null, 2, (PointTransactionReason)999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PointTransactionErrors.InvalidReason);
    }
}
