namespace BookOrbit.Application.SubcutaneousTests.BookCopies.Commands;

using BookOrbit.Application.Features.BookCopies.Commands.CreateBookCopy;
using BookOrbit.Application.Features.BookCopies.Commands.UpdateBookCopy;
using BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeAvilableBookCopy;
using BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeUnAvilableBookCopy;
using BookOrbit.Application.SubcutaneousTests.Books.TestDoubles;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.Books.Enums;
using BookOrbit.Domain.Students.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BookCopyCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateBookCopyCommand_ShouldPersistBookCopy()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        
        var book = StudentTestFactory.CreateBook();
        book.MarkAsAvailable();
        var owner = StudentTestFactory.CreateStudent();
        owner.MarkAsApproved(DateTimeOffset.UtcNow);
        owner.MarkAsActivated();

        context.Books.Add(book);
        context.Students.Add(owner);
        await context.SaveChangesAsync();

        var handler = new CreateBookCopyCommandHandler(
            NullLogger<CreateBookCopyCommandHandler>.Instance,
            context,
            cache,
            routeService);

        var command = new CreateBookCopyCommand(owner.Id, book.Id, BookCopyCondition.LikeNew);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OwnerId.Should().Be(owner.Id);
        result.Value.BookId.Should().Be(book.Id);
        result.Value.Condition.Should().Be(BookCopyCondition.LikeNew);
        result.Value.State.Should().Be(BookCopyState.Available);

        context.BookCopies.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateBookCopyCommand_ShouldUpdateCondition()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook();
        var owner = StudentTestFactory.CreateStudent();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id, BookCopyCondition.New);
        
        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        await context.SaveChangesAsync();

        var handler = new UpdateBookCopyCommandHandler(
            NullLogger<UpdateBookCopyCommandHandler>.Instance,
            context,
            cache);

        var command = new UpdateBookCopyCommand(bookCopy.Id, BookCopyCondition.Acceptable);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(BookCopyCondition.Acceptable);
    }

    [Fact]
    public async Task CreateBookCopyCommand_ShouldReturnError_WhenBookNotAvailable()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        
        var book = StudentTestFactory.CreateBook();
        // Default status is Pending
        var owner = StudentTestFactory.CreateStudent();
        owner.MarkAsApproved(DateTimeOffset.UtcNow);
        owner.MarkAsActivated();

        context.Books.Add(book);
        context.Students.Add(owner);
        await context.SaveChangesAsync();

        var handler = new CreateBookCopyCommandHandler(
            NullLogger<CreateBookCopyCommandHandler>.Instance,
            context,
            cache,
            routeService);

        // Act
        var result = await handler.Handle(new CreateBookCopyCommand(owner.Id, book.Id, BookCopyCondition.New), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Book.BookIsNotAvailable");
    }

    [Fact]
    public async Task MakeUnAvilableBookCopyCommand_ShouldUpdateState()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook();
        var owner = StudentTestFactory.CreateStudent();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id);
        // Default state is Available
        
        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        await context.SaveChangesAsync();

        var handler = new MakeUnAvilableBookCopyCommandHandler(
            context,
            NullLogger<MakeUnAvilableBookCopyCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new MakeUnAvilableBookCopyCommand(bookCopy.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.UnAvailable);
    }

    [Fact]
    public async Task MakeUnAvilableBookCopyCommand_ShouldReturnError_WhenInUse()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook();
        var owner = StudentTestFactory.CreateStudent();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, DateTimeOffset.UtcNow);
        
        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        await context.SaveChangesAsync();

        var handler = new MakeUnAvilableBookCopyCommandHandler(
            context,
            NullLogger<MakeUnAvilableBookCopyCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new MakeUnAvilableBookCopyCommand(bookCopy.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BookCopy.BookCopyInUse");
    }
}
