namespace BookOrbit.Application.UnitTests.Mappers;

using BookOrbit.Application.Features.BorrowingTransactions.Dtos;
using BookOrbit.Domain.BorrowingTransactions;
using BookOrbit.Domain.BorrowingTransactions.Enums;
using FluentAssertions;
using Xunit;

public class BorrowingTransactionMapperTests
{
    [Fact]
    public void FromEntity_ShouldMapScalarFields()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expectedReturnDate = now.AddDays(7);

        var transaction = BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            expectedReturnDate,
            now).Value;

        // Act
        var dto = BorrowingTransactionDto.FromEntity(transaction);

        // Assert
        dto.Id.Should().Be(transaction.Id);
        dto.BorrowingRequestId.Should().Be(transaction.BorrowingRequestId);
        dto.LenderStudentId.Should().Be(transaction.LenderStudentId);
        dto.BorrowerStudentId.Should().Be(transaction.BorrowerStudentId);
        dto.BookCopyId.Should().Be(transaction.BookCopyId);
        dto.State.Should().Be(BorrowingTransactionState.Borrowed);
        dto.ExpectedReturnDate.Should().Be(expectedReturnDate);
        dto.ActualReturnDate.Should().BeNull();
    }
}
