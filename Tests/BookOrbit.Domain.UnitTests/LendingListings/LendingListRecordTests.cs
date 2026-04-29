namespace BookOrbit.Domain.UnitTests.LendingListings;

using BookOrbit.Domain.LendingListings;
using BookOrbit.Domain.LendingListings.Enums;
using BookOrbit.Domain.PointTransactions.ValueObjects;
using FluentAssertions;
using Xunit;

public class LendingListRecordTests
{
    private static LendingListRecord CreateValidRecord(
        DateTimeOffset? currentTime = null,
        DateTimeOffset? expirationDateUtc = null,
        int? borrowingDuration = null,
        Point? cost = null)
    {
        var now = currentTime ?? DateTimeOffset.UtcNow;
        var expiration = expirationDateUtc ?? now.AddDays(1);
        var duration = borrowingDuration ?? LendingListRecord.MinBorrowingDurationInDays;
        var pointCost = cost ?? new Point(LendingListRecord.MinCostInPoints);

        return LendingListRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            duration,
            pointCost,
            expiration,
            now).Value;
    }

    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expiration = now.AddDays(3);
        var id = Guid.NewGuid();
        var bookCopyId = Guid.NewGuid();
        var duration = LendingListRecord.MinBorrowingDurationInDays;
        var cost = new Point(10);

        // Act
        var result = LendingListRecord.Create(id, bookCopyId, duration, cost, expiration, now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.BookCopyId.Should().Be(bookCopyId);
        result.Value.BorrowingDurationInDays.Should().Be(duration);
        result.Value.Cost.Should().Be(cost);
        result.Value.State.Should().Be(LendingListRecordState.Available);
        result.Value.ExpirationDateUtc.Should().Be(expiration);
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = LendingListRecord.Create(Guid.Empty, Guid.NewGuid(), 7, new Point(1), now.AddDays(1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(LendingListRecordErrors.IdRequired);
    }

    [Fact]
    public void Create_WithEmptyBookCopyId_ReturnsBookCopyIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = LendingListRecord.Create(Guid.NewGuid(), Guid.Empty, 7, new Point(1), now.AddDays(1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(LendingListRecordErrors.BookCopyIdRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Create_WithInvalidBorrowingDuration_ReturnsInvalidBorrowingDurationError(int duration)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = LendingListRecord.Create(Guid.NewGuid(), Guid.NewGuid(), duration, new Point(1), now.AddDays(1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(LendingListRecordErrors.InvalidBorrowingDuration);
    }

    [Fact]
    public void Create_WithNullCost_ReturnsCostRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = LendingListRecord.Create(Guid.NewGuid(), Guid.NewGuid(), 7, null!, now.AddDays(1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(LendingListRecordErrors.CostRequeired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2000)]
    public void Create_WithInvalidCost_ReturnsInvalidCostInPointsError(int costValue)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var cost = new Point(costValue <= 0 ? 1 : costValue);

        // Act
        var result = LendingListRecord.Create(Guid.NewGuid(), Guid.NewGuid(), 7, costValue <= 0 ? new Point(0) : cost, now.AddDays(1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(LendingListRecordErrors.InvalidCostInPoints);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithExpirationDateNotInFuture_ReturnsInvalidExpirationDateError(int offsetDays)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expiration = now.AddDays(offsetDays);

        // Act
        var result = LendingListRecord.Create(Guid.NewGuid(), Guid.NewGuid(), 7, new Point(1), expiration, now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(LendingListRecordErrors.InvalidExpirationDate);
    }

    [Fact]
    public void Available_Record_CanTransitionToReserved()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var result = record.MarkAsReserved();

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.State.Should().Be(LendingListRecordState.Reserved);
    }

    [Fact]
    public void Available_Record_CanTransitionToClosed()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var result = record.MarkAsClosed();

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.State.Should().Be(LendingListRecordState.Closed);
    }

    [Fact]
    public void Available_Record_CanTransitionToExpired()
    {
        // Arrange
        var record = CreateValidRecord();

        // Act
        var result = record.MarkAsExpired();

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.State.Should().Be(LendingListRecordState.Expired);
    }

    [Fact]
    public void Reserved_Record_CanTransitionToBorrowed()
    {
        // Arrange
        var record = CreateValidRecord();
        record.MarkAsReserved();

        // Act
        var result = record.MarkAsBorrowed();

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.State.Should().Be(LendingListRecordState.Borrowed);
    }

    [Fact]
    public void Reserved_Record_CanTransitionToAvailable()
    {
        // Arrange
        var record = CreateValidRecord();
        record.MarkAsReserved();

        // Act
        var result = record.UpdateState(LendingListRecordState.Available);

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.State.Should().Be(LendingListRecordState.Available);
    }

    [Fact]
    public void Borrowed_Record_CannotTransitionToReserved()
    {
        // Arrange
        var record = CreateValidRecord();
        record.MarkAsReserved();
        record.MarkAsBorrowed();

        // Act
        var result = record.MarkAsReserved();

        // Assert
        result.IsFailure.Should().BeTrue();
        record.State.Should().Be(LendingListRecordState.Borrowed);
    }

    [Fact]
    public void Expired_Record_CannotTransitionToClosed()
    {
        // Arrange
        var record = CreateValidRecord();
        record.MarkAsExpired();

        // Act
        var result = record.MarkAsClosed();

        // Assert
        result.IsFailure.Should().BeTrue();
        record.State.Should().Be(LendingListRecordState.Expired);
    }
}
