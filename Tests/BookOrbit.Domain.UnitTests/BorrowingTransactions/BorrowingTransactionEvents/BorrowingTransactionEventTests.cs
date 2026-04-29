namespace BookOrbit.Domain.UnitTests.BorrowingTransactions.BorrowingTransactionEvents;

using BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;
using BookOrbit.Domain.BorrowingTransactions.Enums;
using FluentAssertions;
using Xunit;

public class BorrowingTransactionEventTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var state = BorrowingTransactionState.Borrowed;

        // Act
        var result = BorrowingTransactionEvent.Create(id, transactionId, state);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.BorrowingTransactionId.Should().Be(transactionId);
        result.Value.State.Should().Be(state);
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        // Act
        var result = BorrowingTransactionEvent.Create(Guid.Empty, transactionId, BorrowingTransactionState.Borrowed);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionEventErrors.IdRequired);
    }

    [Fact]
    public void Create_WithEmptyBorrowingTransactionId_ReturnsBorrowingTransactionIdRequiredError()
    {
        // Act
        var result = BorrowingTransactionEvent.Create(Guid.NewGuid(), Guid.Empty, BorrowingTransactionState.Borrowed);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionEventErrors.BorrowingTransactionIdRequired);
    }

    [Fact]
    public void Create_WithInvalidState_ReturnsInvalidStateError()
    {
        // Act
        var result = BorrowingTransactionEvent.Create(Guid.NewGuid(), Guid.NewGuid(), (BorrowingTransactionState)999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingTransactionEventErrors.InvalidState);
    }
}
