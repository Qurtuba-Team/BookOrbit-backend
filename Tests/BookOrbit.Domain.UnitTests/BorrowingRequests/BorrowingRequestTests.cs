namespace BookOrbit.Domain.UnitTests.BorrowingRequests;

using BookOrbit.Domain.BorrowingRequests;
using BookOrbit.Domain.BorrowingRequests.Enums;
using FluentAssertions;
using Xunit;

public class BorrowingRequestTests
{
    private static BorrowingRequest CreateValidRequest(
        DateTimeOffset? currentTime = null,
        DateTimeOffset? expirationDateUtc = null)
    {
        var now = currentTime ?? DateTimeOffset.UtcNow;
        var expiration = expirationDateUtc ?? now.AddDays(1);

        return BorrowingRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            expiration,
            now).Value;
    }

    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expiration = now.AddDays(1);
        var id = Guid.NewGuid();
        var borrowingStudentId = Guid.NewGuid();
        var lendingRecordId = Guid.NewGuid();

        // Act
        var result = BorrowingRequest.Create(id, borrowingStudentId, lendingRecordId, expiration, now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.BorrowingStudentId.Should().Be(borrowingStudentId);
        result.Value.LendingRecordId.Should().Be(lendingRecordId);
        result.Value.State.Should().Be(BorrowingRequestState.Pending);
        result.Value.ExpirationDateUtc.Should().Be(expiration);
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingRequest.Create(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), now.AddDays(1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingRequestErrors.IdRequired);
    }

    [Fact]
    public void Create_WithEmptyBorrowingStudentId_ReturnsBorrowingStudentIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingRequest.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), now.AddDays(1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingRequestErrors.BorrowingStudentIdRequired);
    }

    [Fact]
    public void Create_WithEmptyLendingRecordId_ReturnsLendingRecordIdRequiredError()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = BorrowingRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, now.AddDays(1), now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingRequestErrors.LendingRecordIdRequired);
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
        var result = BorrowingRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), expiration, now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BorrowingRequestErrors.InvalidExpirationDate);
    }

    [Fact]
    public void Pending_Request_CanTransitionToAccepted()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = request.MarkAsApproved();

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.State.Should().Be(BorrowingRequestState.Accepted);
    }

    [Fact]
    public void Pending_Request_CanTransitionToRejected()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = request.MarkAsRejected();

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.State.Should().Be(BorrowingRequestState.Rejected);
    }

    [Fact]
    public void Pending_Request_CanTransitionToCancelled()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = request.MarkAsCancelled();

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.State.Should().Be(BorrowingRequestState.Cancelled);
    }

    [Fact]
    public void Pending_Request_CanTransitionToExpired()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = request.MarkAsExpired();

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.State.Should().Be(BorrowingRequestState.Expired);
    }

    [Fact]
    public void Accepted_Request_CanTransitionToCancelled()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MarkAsApproved();

        // Act
        var result = request.MarkAsCancelled();

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.State.Should().Be(BorrowingRequestState.Cancelled);
    }

    [Fact]
    public void Accepted_Request_CanTransitionToExpired()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MarkAsApproved();

        // Act
        var result = request.MarkAsExpired();

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.State.Should().Be(BorrowingRequestState.Expired);
    }

    [Fact]
    public void Rejected_Request_CannotTransitionToAccepted()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MarkAsRejected();

        // Act
        var result = request.MarkAsApproved();

        // Assert
        result.IsFailure.Should().BeTrue();
        request.State.Should().Be(BorrowingRequestState.Rejected);
    }

    [Fact]
    public void Cancelled_Request_CannotTransitionToExpired()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MarkAsCancelled();

        // Act
        var result = request.MarkAsExpired();

        // Assert
        result.IsFailure.Should().BeTrue();
        request.State.Should().Be(BorrowingRequestState.Cancelled);
    }
}
