namespace BookOrbit.Application.SubcutaneousTests.Books.Commands;

using BookOrbit.Application.Features.Books.Commands.CreateBook;
using BookOrbit.Application.Features.Books.Commands.UpdateBook;
using BookOrbit.Application.Features.Books.Commands.DeleteBook;
using BookOrbit.Application.Features.Books.Commands.StateMachien.MakeBookAvilable;
using BookOrbit.Application.Features.Books.Commands.StateMachien.RejectBook;
using BookOrbit.Application.SubcutaneousTests.Books.TestDoubles;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.Books;
using BookOrbit.Domain.Books.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using BookOrbit.Infrastructure.Services.ConcurrencyServices;
using BookOrbit.Infrastructure.Common.Errors;
using BookOrbit.Domain.Common.Results;
using Xunit;

public class BookCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateBookCommand_ShouldPersistBook()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        var handler = new CreateBookCommandHandler(
            NullLogger<CreateBookCommandHandler>.Instance,
            context,
            cache,
            routeService);

        var command = new CreateBookCommand(
            Title: "Test Driven Development",
            ISBN: "9780321146533",
            Publisher: "Addison Wesley Professional",
            Category: BookCategory.Science,
            Author: "Kent Beck",
            CoverImageFileName: "tdd.png");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(command.Title);
        result.Value.ISBN.Should().Be(command.ISBN);
        result.Value.Status.Should().Be(BookStatus.Pending);

        context.Books.Should().HaveCount(1);
        context.Books.Single().Title.Value.Should().Be(command.Title);
    }

    [Fact]
    public async Task UpdateBookCommand_ShouldUpdateBookDetails()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook(title: "Old Title");
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var concurrencyService = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new UpdateBookCommandHandler(
            NullLogger<UpdateBookCommandHandler>.Instance,
            context,
            concurrencyService,
            cache);

        var command = new UpdateBookCommand(
            Id: book.Id,
            Title: "New Title",
            BookCoverImageFileName: "new-cover.png",
            RowVersion: Convert.ToBase64String(new byte[] {1,2,3}));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.Title.Value.Should().Be("New Title");
        book.CoverImageFileName.Should().Be("new-cover.png");
    }

    [Fact]
    public async Task MakeBookAvilableCommand_ShouldUpdateStatusToAvailable()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook();
        // Default status is Pending
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var concurrencyService = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new MakeBookAvilableCommandHandler(
            context,
            NullLogger<MakeBookAvilableCommandHandler>.Instance,
            cache,
            concurrencyService);

        // Act
        var result = await handler.Handle(new MakeBookAvilableCommand(book.Id, Convert.ToBase64String(new byte[] {1,2,3})), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.Status.Should().Be(BookStatus.Available);
    }

    [Fact]
    public async Task RejectBookCommand_ShouldUpdateStatusToRejected()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook();
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var concurrencyService = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new RejectBookCommandHandler(
            context,
            concurrencyService,
            NullLogger<RejectBookCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new RejectBookCommand(book.Id, Convert.ToBase64String(new byte[] {1,2,3})), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.Status.Should().Be(BookStatus.Rejected);
    }

    [Fact]
    public async Task DeleteBookCommand_ShouldRemoveBook_WhenNotUsed()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var currentUser = new FakeCurrentUser();
        var book = StudentTestFactory.CreateBook();
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var handler = new DeleteBookCommandHandler(
            NullLogger<DeleteBookCommandHandler>.Instance,
            context,
            cache,
            currentUser);

        // Act
        var result = await handler.Handle(new DeleteBookCommand(book.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Books.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteBookCommand_ShouldReturnError_WhenBookIsUsed()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var currentUser = new FakeCurrentUser();
        var book = StudentTestFactory.CreateBook();
        var owner = StudentTestFactory.CreateStudent();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id);
        
        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        await context.SaveChangesAsync();

        var handler = new DeleteBookCommandHandler(
            NullLogger<DeleteBookCommandHandler>.Instance,
            context,
            cache,
            currentUser);

        // Act
        var result = await handler.Handle(new DeleteBookCommand(book.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Book.IsUsedByBookCopies");
    }

    [Theory]
    [MemberData(nameof(ConcurrencyTokenRequiredScenarios))]
    public async Task Commands_ShouldReturnConcurrencyTokenRequired_WhenRowVersionIsMissing(
        string scenario,
        Func<Task<Result<Updated>>> act)
    {
        var result = await act();

        result.IsFailure.Should().BeTrue($"{scenario} should fail when the concurrency token is missing");
        result.Errors.Should().Contain(e => e.Code == InfrastrucureConcurrencyErrors.ConcurrencyTokenRequired.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidConcurrencyFormatScenarios))]
    public async Task Commands_ShouldReturnInvalidConcurrencyFormat_WhenRowVersionIsMalformed(
        string scenario,
        Func<Task<Result<Updated>>> act)
    {
        var result = await act();

        result.IsFailure.Should().BeTrue($"{scenario} should fail when the concurrency token is malformed");
        result.Errors.Should().Contain(e => e.Code == InfrastrucureConcurrencyErrors.InvalidConcurrencyFormat.Code);
    }

    [Theory]
    [MemberData(nameof(ConcurrencyConflictScenarios))]
    public async Task Commands_ShouldThrowDbUpdateConcurrencyException_WhenRowVersionConflicts(
        string scenario,
        Func<Task<Result<Updated>>> act)
    {
        var action = async () => await act();

        await action.Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>($"{scenario} should throw when the row version conflicts");
    }

    public static IEnumerable<object[]> ConcurrencyTokenRequiredScenarios()
    {
        yield return new object[] { nameof(MakeBookAvilableCommandHandler), CreateMakeBookAvilableExecution(null) };
        yield return new object[] { nameof(RejectBookCommandHandler), CreateRejectBookExecution(null) };
        yield return new object[] { nameof(UpdateBookCommandHandler), CreateUpdateBookExecution(null) };
    }

    public static IEnumerable<object[]> InvalidConcurrencyFormatScenarios()
    {
        const string invalidRowVersion = "not-a-base64-token";

        yield return new object[] { nameof(MakeBookAvilableCommandHandler), CreateMakeBookAvilableExecution(invalidRowVersion) };
        yield return new object[] { nameof(RejectBookCommandHandler), CreateRejectBookExecution(invalidRowVersion) };
        yield return new object[] { nameof(UpdateBookCommandHandler), CreateUpdateBookExecution(invalidRowVersion) };
    }

    public static IEnumerable<object[]> ConcurrencyConflictScenarios()
    {
        string conflictRowVersion = Convert.ToBase64String(new byte[] { 9, 9, 9 });

        yield return new object[] { nameof(MakeBookAvilableCommandHandler), CreateMakeBookAvilableExecution(conflictRowVersion) };
        yield return new object[] { nameof(RejectBookCommandHandler), CreateRejectBookExecution(conflictRowVersion) };
        yield return new object[] { nameof(UpdateBookCommandHandler), CreateUpdateBookExecution(conflictRowVersion) };
    }

    private static Func<Task<Result<Updated>>> CreateMakeBookAvilableExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var book = StudentTestFactory.CreateBook();
            // Default status is Pending
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var handler = new MakeBookAvilableCommandHandler(
                context,
                NullLogger<MakeBookAvilableCommandHandler>.Instance,
                cache,
                concurrencyService);

            return await handler.Handle(new MakeBookAvilableCommand(book.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateRejectBookExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var book = StudentTestFactory.CreateBook();
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var handler = new RejectBookCommandHandler(
                context,
                concurrencyService,
                NullLogger<RejectBookCommandHandler>.Instance,
                cache);

            return await handler.Handle(new RejectBookCommand(book.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateUpdateBookExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var book = StudentTestFactory.CreateBook(title: "Old Title");
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var handler = new UpdateBookCommandHandler(
                NullLogger<UpdateBookCommandHandler>.Instance,
                context,
                concurrencyService,
                cache);

            return await handler.Handle(new UpdateBookCommand(book.Id, "New Title", "new-cover.png", rowVersion!), CancellationToken.None);
        };
}
