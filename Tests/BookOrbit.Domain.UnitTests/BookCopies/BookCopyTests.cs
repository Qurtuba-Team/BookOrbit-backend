namespace BookOrbit.Domain.UnitTests.BookCopies;

using BookOrbit.Domain.BookCopies;
using BookOrbit.Domain.BookCopies.Enums;
using FluentAssertions;
using Xunit;

public class BookCopyTests
{
    #region Constants

    // (No constants needed - all test values are one-off)

    #endregion

    #region Test Helpers

    private static BookCopy CreateValidBookCopy(
        Guid? id = null,
        Guid? ownerId = null,
        Guid? bookId = null,
        BookCopyCondition condition = BookCopyCondition.New)
    {
        var bookCopyId = id ?? Guid.NewGuid();
        var validOwnerId = ownerId ?? Guid.NewGuid();
        var validBookId = bookId ?? Guid.NewGuid();

        return BookCopy.Create(bookCopyId, validOwnerId, validBookId, condition).Value;
    }

    private static void SetBookCopyState(BookCopy bookCopy, BookCopyState state)
    {
        var property = typeof(BookCopy).GetProperty("State", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

        if (property != null && property.CanWrite)
        {
            property.SetValue(bookCopy, state);
            return;
        }

        var field = typeof(BookCopy).GetField("<State>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(bookCopy, state);
    }

    #endregion

    #region Create Method - Success Tests

    [Fact]
    public void Create_WithAllRequiredValidParameters_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var condition = BookCopyCondition.New;

        // Act
        var result = BookCopy.Create(id, ownerId, bookId, condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.OwnerId.Should().Be(ownerId);
        result.Value.BookId.Should().Be(bookId);
        result.Value.Condition.Should().Be(condition);
        result.Value.State.Should().Be(BookCopyState.Available);
    }

    [Theory]
    [InlineData(BookCopyCondition.New)]
    [InlineData(BookCopyCondition.LikeNew)]
    [InlineData(BookCopyCondition.Acceptable)]
    [InlineData(BookCopyCondition.Poor)]
    public void Create_WithDifferentConditions_ReturnsSuccessResult(BookCopyCondition condition)
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        // Act
        var result = BookCopy.Create(id, ownerId, bookId, condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Condition.Should().Be(condition);
    }

    [Fact]
    public void Create_WithValidParameters_StateIsAvailable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var condition = BookCopyCondition.LikeNew;

        // Act
        var result = BookCopy.Create(id, ownerId, bookId, condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(BookCopyState.Available);
    }

    [Theory]
    [InlineData(BookCopyCondition.New)]
    [InlineData(BookCopyCondition.LikeNew)]
    [InlineData(BookCopyCondition.Acceptable)]
    public void Create_WithMultipleValidConditions_AllHaveAvailableState(BookCopyCondition condition)
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        // Act
        var result = BookCopy.Create(id, ownerId, bookId, condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(BookCopyState.Available);
    }

    #endregion

    #region Create Method - Null/Empty Parameter Tests

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var condition = BookCopyCondition.New;

        // Act
        var result = BookCopy.Create(Guid.Empty, ownerId, bookId, condition);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookCopyErrors.IdRequired);
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ReturnsOwnerIdRequiredError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var condition = BookCopyCondition.New;

        // Act
        var result = BookCopy.Create(id, Guid.Empty, bookId, condition);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookCopyErrors.OwnerIdRequired);
    }

    [Fact]
    public void Create_WithEmptyBookId_ReturnsBookIdRequiredError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var condition = BookCopyCondition.New;

        // Act
        var result = BookCopy.Create(id, ownerId, Guid.Empty, condition);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookCopyErrors.BookIdRequired);
    }

    #endregion

    #region Create Method - Invalid Condition Tests

    [Fact]
    public void Create_WithInvalidCondition_ReturnsInvalidConditionError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var invalidCondition = (BookCopyCondition)999;  // Invalid enum value

        // Act
        var result = BookCopy.Create(id, ownerId, bookId, invalidCondition);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookCopyErrors.InvalidCondition);
    }

    #endregion

    #region Update Method - Success Tests

    [Fact]
    public void Update_WithValidCondition_UpdatesCondition()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy(condition: BookCopyCondition.New);
        var newCondition = BookCopyCondition.Acceptable;

        // Act
        var result = bookCopy.Update(newCondition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(newCondition);
    }

    [Theory]
    [InlineData(BookCopyCondition.New, BookCopyCondition.LikeNew)]
    [InlineData(BookCopyCondition.LikeNew, BookCopyCondition.Acceptable)]
    [InlineData(BookCopyCondition.Acceptable, BookCopyCondition.Poor)]
    [InlineData(BookCopyCondition.Poor, BookCopyCondition.New)]
    public void Update_WithDifferentValidConditions_UpdatesCorrectly(
        BookCopyCondition initialCondition,
        BookCopyCondition newCondition)
    {
        // Arrange
        var bookCopy = CreateValidBookCopy(condition: initialCondition);

        // Act
        var result = bookCopy.Update(newCondition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(newCondition);
        bookCopy.Condition.Should().NotBe(initialCondition);
    }

    [Fact]
    public void Update_WithValidCondition_DoesNotChangeOtherProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var bookCopy = CreateValidBookCopy(id: id, ownerId: ownerId, bookId: bookId, condition: BookCopyCondition.New);
        
        var originalId = bookCopy.Id;
        var originalOwnerId = bookCopy.OwnerId;
        var originalBookId = bookCopy.BookId;
        var originalState = bookCopy.State;

        var newCondition = BookCopyCondition.Acceptable;

        // Act
        var result = bookCopy.Update(newCondition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Id.Should().Be(originalId);
        bookCopy.OwnerId.Should().Be(originalOwnerId);
        bookCopy.BookId.Should().Be(originalBookId);
        bookCopy.State.Should().Be(originalState);
    }

    [Theory]
    [InlineData(BookCopyCondition.New)]
    [InlineData(BookCopyCondition.LikeNew)]
    [InlineData(BookCopyCondition.Acceptable)]
    [InlineData(BookCopyCondition.Poor)]
    public void Update_WithAllValidConditions_ReturnsSuccess(BookCopyCondition condition)
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        var newCondition = condition;

        // Act
        var result = bookCopy.Update(newCondition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(newCondition);
    }

    #endregion

    #region Update Method - Invalid Condition Tests

    [Fact]
    public void Update_WithInvalidCondition_ReturnsInvalidConditionError()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        var invalidCondition = (BookCopyCondition)999;  // Invalid enum value

        // Act
        var result = bookCopy.Update(invalidCondition);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookCopyErrors.InvalidCondition);
    }


    #endregion

    #region BookCopy Properties Tests

    [Fact]
    public void Create_WithValidParameters_AllPropertiesAreSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var condition = BookCopyCondition.LikeNew;

        // Act
        var result = BookCopy.Create(id, ownerId, bookId, condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var bookCopy = result.Value;

        bookCopy.Id.Should().Be(id);
        bookCopy.OwnerId.Should().Be(ownerId);
        bookCopy.BookId.Should().Be(bookId);
        bookCopy.Condition.Should().Be(condition);
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    [Fact]
    public void Create_WithValidParameters_OwnerAndBookAreNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var condition = BookCopyCondition.New;

        // Act
        var result = BookCopy.Create(id, ownerId, bookId, condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Owner.Should().BeNull();
        result.Value.Book.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidParameters_OwnerIdIsReadOnly()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        var originalOwnerId = bookCopy.OwnerId;

        // Act & Assert - OwnerID cannot be changed (it's a property getter without setter)
        bookCopy.OwnerId.Should().Be(originalOwnerId);
    }

    [Fact]
    public void Create_WithValidParameters_BookIdIsReadOnly()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        var originalBookId = bookCopy.BookId;

        // Act & Assert - BookId cannot be changed
        bookCopy.BookId.Should().Be(originalBookId);
    }

    [Fact]
    public void Create_WithValidParameters_StateIsReadOnly()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        var originalState = bookCopy.State;

        // Act & Assert - State cannot be changed (it's a property getter without setter)
        bookCopy.State.Should().Be(originalState);
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_WithMultipleBookCopiesWithDifferentIds_EachHasUniqueId()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var condition = BookCopyCondition.New;

        // Act
        var result1 = BookCopy.Create(id1, ownerId, bookId, condition);
        var result2 = BookCopy.Create(id2, ownerId, bookId, condition);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);
    }

    [Fact]
    public void Create_WithSameBookButDifferentOwners_CreatesMultipleCopies()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var ownerId1 = Guid.NewGuid();
        var ownerId2 = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var condition = BookCopyCondition.New;

        // Act
        var result1 = BookCopy.Create(id1, ownerId1, bookId, condition);
        var result2 = BookCopy.Create(id2, ownerId2, bookId, condition);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.BookId.Should().Be(result2.Value.BookId);
        result1.Value.OwnerId.Should().NotBe(result2.Value.OwnerId);
    }

    [Fact]
    public void Create_WithSameOwnerButDifferentBooks_CreatesMultipleCopies()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var bookId1 = Guid.NewGuid();
        var bookId2 = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var condition = BookCopyCondition.New;

        // Act
        var result1 = BookCopy.Create(id1, ownerId, bookId1, condition);
        var result2 = BookCopy.Create(id2, ownerId, bookId2, condition);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.OwnerId.Should().Be(result2.Value.OwnerId);
        result1.Value.BookId.Should().NotBe(result2.Value.BookId);
    }

    [Fact]
    public void Update_MultipleTimesWithDifferentConditions_UpdatesCorrectly()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy(condition: BookCopyCondition.New);
        var secondCondition = BookCopyCondition.LikeNew;
        var thirdCondition = BookCopyCondition.Acceptable;

        // Act
        var result1 = bookCopy.Update(secondCondition);
        bookCopy.Condition.Should().Be(secondCondition);

        var result2 = bookCopy.Update(thirdCondition);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(thirdCondition);
    }

    [Fact]
    public void Create_WithValidParameters_StateIsAlwaysAvailable()
    {
        // Arrange
        var bookCopies = new[]
        {
            BookCopy.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BookCopyCondition.New),
            BookCopy.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BookCopyCondition.LikeNew),
            BookCopy.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BookCopyCondition.Acceptable),
            BookCopy.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BookCopyCondition.Poor)
        };

        // Act & Assert
        foreach (var result in bookCopies)
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.State.Should().Be(BookCopyState.Available);
        }
    }

    #endregion

    #region Result Type Tests

    [Fact]
    public void Update_WithValidCondition_ReturnsUpdatedResult()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        var newCondition = BookCopyCondition.Acceptable;

        // Act
        var result = bookCopy.Update(newCondition);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Condition Transition Tests

    [Theory]
    [InlineData(BookCopyCondition.New, BookCopyCondition.LikeNew)]
    [InlineData(BookCopyCondition.New, BookCopyCondition.Acceptable)]
    [InlineData(BookCopyCondition.New, BookCopyCondition.Poor)]
    [InlineData(BookCopyCondition.LikeNew, BookCopyCondition.Acceptable)]
    [InlineData(BookCopyCondition.LikeNew, BookCopyCondition.Poor)]
    [InlineData(BookCopyCondition.Acceptable, BookCopyCondition.Poor)]
    public void Update_FromGoodToWorseCondition_UpdatesSuccessfully(
        BookCopyCondition initialCondition,
        BookCopyCondition worseCondition)
    {
        // Arrange
        var bookCopy = CreateValidBookCopy(condition: initialCondition);

        // Act
        var result = bookCopy.Update(worseCondition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(worseCondition);
    }

    [Theory]
    [InlineData(BookCopyCondition.Poor, BookCopyCondition.New)]
    [InlineData(BookCopyCondition.Poor, BookCopyCondition.LikeNew)]
    [InlineData(BookCopyCondition.Acceptable, BookCopyCondition.New)]
    [InlineData(BookCopyCondition.Acceptable, BookCopyCondition.LikeNew)]
    [InlineData(BookCopyCondition.LikeNew, BookCopyCondition.New)]
    public void Update_FromWorseToGoodCondition_UpdatesSuccessfully(
        BookCopyCondition initialCondition,
        BookCopyCondition betterCondition)
    {
        // Arrange
        var bookCopy = CreateValidBookCopy(condition: initialCondition);

        // Act
        var result = bookCopy.Update(betterCondition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(betterCondition);
    }

    [Theory]
    [InlineData(BookCopyCondition.New)]
    [InlineData(BookCopyCondition.LikeNew)]
    [InlineData(BookCopyCondition.Acceptable)]
    [InlineData(BookCopyCondition.Poor)]
    public void Update_WithSameCondition_UpdatesSuccessfully(BookCopyCondition condition)
    {
        // Arrange
        var bookCopy = CreateValidBookCopy(condition: condition);

        // Act
        var result = bookCopy.Update(condition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(condition);
    }

    #endregion

    #region State Transition - Available State Tests

    [Fact]
    public void Available_BookCopy_CanTransitionToUnAvilable()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();

        // Act
        var result = bookCopy.MarkAsUnAvilable();

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.UnAvailable);
    }

    [Fact]
    public void Available_BookCopy_CannotTransitionToBorrowed()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();

        // Act
        var result = bookCopy.MarkAsBorrowed();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    [Fact]
    public void Available_BookCopy_CannotTransitionToLost()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();

        // Act
        var result = bookCopy.MarkAsLost();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    [Fact]
    public void Available_BookCopy_CannotTransitionToAvailable()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();

        // Act
        var result = bookCopy.MarkAsAvilable();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    #endregion

    #region State Transition - Borrowed State Tests

    [Fact]
    public void Borrowed_BookCopy_CanTransitionToAvailable()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        SetBookCopyState(bookCopy, BookCopyState.Borrowed);

        // Act
        var result = bookCopy.MarkAsAvilable();

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    [Fact]
    public void Borrowed_BookCopy_CanTransitionToLost()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        SetBookCopyState(bookCopy, BookCopyState.Borrowed);

        // Act
        var result = bookCopy.MarkAsLost();

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Lost);
    }

    [Fact]
    public void Borrowed_BookCopy_CannotTransitionToUnAvilable()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        SetBookCopyState(bookCopy, BookCopyState.Borrowed);

        // Act
        var result = bookCopy.MarkAsUnAvilable();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Borrowed);
    }

    [Fact]
    public void Borrowed_BookCopy_CannotTransitionToBorrowed()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        SetBookCopyState(bookCopy, BookCopyState.Borrowed);

        // Act
        var result = bookCopy.MarkAsBorrowed();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Borrowed);
    }

    #endregion

    #region State Transition - Lost State Tests

    [Fact]
    public void Lost_BookCopy_CanTransitionToAvailable()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        SetBookCopyState(bookCopy, BookCopyState.Lost);

        // Act
        var result = bookCopy.MarkAsAvilable();

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    [Fact]
    public void Lost_BookCopy_CanTransitionToUnAvilable()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        SetBookCopyState(bookCopy, BookCopyState.Lost);

        // Act
        var result = bookCopy.MarkAsUnAvilable();

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.UnAvailable);
    }

    [Fact]
    public void Lost_BookCopy_CannotTransitionToBorrowed()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        SetBookCopyState(bookCopy, BookCopyState.Lost);

        // Act
        var result = bookCopy.MarkAsBorrowed();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Lost);
    }

    [Fact]
    public void Lost_BookCopy_CannotTransitionToLost()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        SetBookCopyState(bookCopy, BookCopyState.Lost);

        // Act
        var result = bookCopy.MarkAsLost();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Lost);
    }

    #endregion

    #region State Transition - UnAvilable State Tests

    [Fact]
    public void UnAvilable_BookCopy_CanTransitionToAvailable()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        bookCopy.MarkAsUnAvilable();

        // Act
        var result = bookCopy.MarkAsAvilable();

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    [Fact]
    public void UnAvilable_BookCopy_CannotTransitionToBorrowed()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        bookCopy.MarkAsUnAvilable();

        // Act
        var result = bookCopy.MarkAsBorrowed();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.UnAvailable);
    }

    [Fact]
    public void UnAvilable_BookCopy_CannotTransitionToLost()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        bookCopy.MarkAsUnAvilable();

        // Act
        var result = bookCopy.MarkAsLost();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.UnAvailable);
    }

    [Fact]
    public void UnAvilable_BookCopy_CannotTransitionToUnAvilable()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        bookCopy.MarkAsUnAvilable();

        // Act
        var result = bookCopy.MarkAsUnAvilable();

        // Assert
        result.IsFailure.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.UnAvailable);
    }

    #endregion

    #region Complete State Flow Tests

    [Fact]
    public void CompleteFlow_Available_UnAvilable_Available()
    {
        // Arrange
        var bookCopy = CreateValidBookCopy();
        bookCopy.State.Should().Be(BookCopyState.Available);

        // Act & Assert - Unavailable
        var unavailableResult = bookCopy.MarkAsUnAvilable();
        unavailableResult.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.UnAvailable);

        // Act & Assert - Available
        var availableResult = bookCopy.MarkAsAvilable();
        availableResult.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    #endregion
}
