namespace BookOrbit.Domain.UnitTests.BorrowingTransactions;

using BookOrbit.Domain.BorrowingTransactions;
using BookOrbit.Domain.BorrowingTransactions.Enums;
using BookOrbit.Domain.UnitTests.Helpers;
using FluentAssertions;
using Xunit;

public class BorrowingTransactionTests
{
    private static BorrowingTransaction CreateValidTransaction(
        DateTimeOffset? currentTime = null,
        DateTimeOffset? expectedReturnDate = null,
        Guid? lenderStudentId = null,
        Guid? borrowerStudentId = null)
    {
        var now = currentTime ?? DateTimeOffset.UtcNow;
        var expectedReturn = expectedReturnDate ?? now.AddDays(7);

        return BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            lenderStudentId ?? Guid.NewGuid(),
            borrowerStudentId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            expectedReturn,
            now).Value;
    }

    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expectedReturnDate = now.AddDays(7);
        var id = Guid.NewGuid();
        var borrowingRequestId = Guid.NewGuid();
        var lenderId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var bookCopyId = Guid.NewGuid();

        // Act
        var result = BorrowingTransaction.Create(
            id,
            borrowingRequestId,
            lenderId,
            borrowerId,
            bookCopyId,
            expectedReturnDate,
            now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.BorrowingRequestId.Should().Be(borrowingRequestId);
        result.Value.LenderStudentId.Should().Be(lenderId);
        result.Value.BorrowerStudentId.Should().Be(borrowerId);
        result.Value.BookCopyId.Should().Be(bookCopyId);
        result.Value.State.Should().Be(BorrowingTransactionState.Borrowed);
        result.Value.ExpectedReturnDate.Should().Be(expectedReturnDate);
        result.Value.ActualReturnDate.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingTransaction.Create(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(3),
            now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.IdRequired);
    }

    [Fact]
    public void Create_WithEmptyBorrowingRequestId_ReturnsBorrowingRequestIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(3),
            now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.BorrowingRequestIdRequired);
    }

    [Fact]
    public void Create_WithEmptyLenderStudentId_ReturnsLenderStudentIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(3),
            now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.LenderStudentIdRequired);
    }

    [Fact]
    public void Create_WithEmptyBorrowerStudentId_ReturnsBorrowerStudentIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            now.AddDays(3),
            now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.BorrowerStudentIdRequired);
    }

    [Fact]
    public void Create_WithEmptyBookCopyId_ReturnsBookCopyIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            now.AddDays(3),
            now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.BookCopyIdRequired);
    }

    [Fact]
    public void Create_WithExpectedReturnDateNotInFuture_ReturnsInvalidExpectedReturnDateError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now,
            now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.InvalidExpectedReturnDate);
    }

    [Fact]
    public void Create_WithSameLenderAndBorrower_ReturnsLenderAndBorrowerCannotBeTheSameError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var studentId = Guid.NewGuid();

        // Act
        var result = BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            studentId,
            studentId,
            Guid.NewGuid(),
            now.AddDays(7),
            now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.LenderAndBorrowerCannotBeTheSame);
    }

    [Fact]
    public void ReturnBookCopy_WithReturnDateBeforeCreation_ReturnsReturnDateShouldBeAfterCreationDateError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var transaction = CreateValidTransaction(now);
        transaction.SetCreatedAt(now);

        // Act
        var result = transaction.ReturnBookCopy(now.AddMinutes(-1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.ReturnDateShouldBeAfterCreationDate);
    }

    [Fact]
    public void ReturnBookCopy_WithReturnDateInFuture_ReturnsReturnDateCannotBeInTheFutureError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var transaction = CreateValidTransaction(now);
        transaction.SetCreatedAt(now.AddMinutes(-10));

        // Act
        var result = transaction.ReturnBookCopy(now.AddMinutes(5), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.ReturnDateCannotBeInTheFuture);
    }

    [Fact]
    public void ReturnBookCopy_WithReturnDateOnTime_SetsReturnedState()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expectedReturn = now.AddDays(2);
        var transaction = CreateValidTransaction(now, expectedReturn);
        transaction.SetCreatedAt(now.AddMinutes(-1));

        var returnDate = now.AddDays(1);

        // Act
        var result = transaction.ReturnBookCopy(returnDate, returnDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Returned);
        transaction.ActualReturnDate.Should().Be(returnDate);
    }

    [Fact]
    public void ReturnBookCopy_WithLateReturn_SetsOverdueState()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expectedReturn = now.AddDays(1);
        var transaction = CreateValidTransaction(now, expectedReturn);
        transaction.SetCreatedAt(now.AddMinutes(-1));

        var returnDate = now.AddDays(2);

        // Act
        var result = transaction.ReturnBookCopy(returnDate, now.AddDays(2));

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Overdue);
        transaction.ActualReturnDate.Should().Be(returnDate);
    }

    [Fact]
    public void MarkAsOverdue_WhenExpectedReturnDateNotPassed_ReturnsCannotMarkOverdueError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var transaction = CreateValidTransaction(now, now.AddDays(2));

        // Act
        var result = transaction.MarkAsOverdue(now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionErrors.CannotMarkOverdueWhileExpectedReturnDateNotPast);
    }

    [Fact]
    public void MarkAsOverdue_WhenExpectedReturnDatePassed_UpdatesStateToOverdue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var transaction = CreateValidTransaction(now, now.AddDays(1));

        // Act
        var result = transaction.MarkAsOverdue(now.AddDays(2));

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Overdue);
    }

    [Fact]
    public void MarkAsLost_FromBorrowed_UpdatesStateToLost()
    {
        // Arrange
        var transaction = CreateValidTransaction();

        // Act
        var result = transaction.MarkAsLost();

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Lost);
    }

    [Fact]
    public void MarkAsLost_FromReturned_ReturnsFailure()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var transaction = CreateValidTransaction(now, now.AddDays(2));
        transaction.SetCreatedAt(now.AddMinutes(-1));
        transaction.ReturnBookCopy(now.AddDays(1), now.AddDays(1));

        // Act
        var result = transaction.MarkAsLost();

        // Assert
        result.IsFailure.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Returned);
    }

    [Fact]
    public void MarkAsBorrowed_WhenAlreadyBorrowed_ReturnsUpdated()
    {
        // Arrange
        var transaction = CreateValidTransaction();

        // Act
        var result = transaction.MarkAsBorrowed();

        // Assert
        result.IsFailure.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Borrowed);

    }

    [Fact]
    public void MarkAsBorrowed_FromReturned_ReturnsFailure()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var transaction = CreateValidTransaction(now, now.AddDays(2));
        transaction.SetCreatedAt(now.AddMinutes(-1));
        transaction.ReturnBookCopy(now.AddDays(1), now.AddDays(1));

        // Act
        var result = transaction.MarkAsBorrowed();

        // Assert
        result.IsFailure.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Returned);
    }
}
