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
using Xunit;
using System.Linq;

public class BookCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateBookCommand_ShouldPersistBook()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        var coverRetrievalService = new FakeBookCoverRetrievalService();
        var handler = new CreateBookCommandHandler(
            NullLogger<CreateBookCommandHandler>.Instance,
            context,
            cache,
            routeService,
            coverRetrievalService);

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
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(command.Title);
        result.Value.ISBN.Should().Be(command.ISBN);
        result.Value.Status.Should().Be(BookStatus.Pending);

        context.Books.Should().HaveCount(1);
        context.Books.Single().Title.Value.Should().Be(command.Title);
    }

    [Fact]
    public async Task CreateBookCommand_WithoutCover_ShouldAutoRetrieveCover()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        var coverRetrievalService = new FakeBookCoverRetrievalService();
        var handler = new CreateBookCommandHandler(
            NullLogger<CreateBookCommandHandler>.Instance,
            context,
            cache,
            routeService,
            coverRetrievalService);

        var command = new CreateBookCommand(
            Title: "Clean Code",
            ISBN: "9780132350884",
            Publisher: "Prentice Hall",
            Category: BookCategory.Science,
            Author: "Robert Martin",
            CoverImageFileName: null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        coverRetrievalService.CallCount.Should().Be(1);
        result.Value.BookCoverImageUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateBookCommand_WithCover_ShouldSkipRetrievalService()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        var coverRetrievalService = new FakeBookCoverRetrievalService();
        var handler = new CreateBookCommandHandler(
            NullLogger<CreateBookCommandHandler>.Instance,
            context,
            cache,
            routeService,
            coverRetrievalService);

        var command = new CreateBookCommand(
            Title: "The Pragmatic Programmer",
            ISBN: "9780135957059",
            Publisher: "Addison Wesley Professional",
            Category: BookCategory.Science,
            Author: "David Thomas",
            CoverImageFileName: "pragmatic.png");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        coverRetrievalService.CallCount.Should().Be(0);
        result.Value.BookCoverImageUrl.Should().Be("http://localhost/images/pragmatic.png");
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

        var handler = new UpdateBookCommandHandler(
            NullLogger<UpdateBookCommandHandler>.Instance,
            context,
            cache);

        var command = new UpdateBookCommand(
            Id: book.Id,
            Title: "New Title",
            BookCoverImageFileName: "new-cover.png");

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

        var handler = new MakeBookAvilableCommandHandler(
            context,
            NullLogger<MakeBookAvilableCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new MakeBookAvilableCommand(book.Id), CancellationToken.None);

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

        var handler = new RejectBookCommandHandler(
            context,
            NullLogger<RejectBookCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new RejectBookCommand(book.Id), CancellationToken.None);

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
    [Fact]
    public async Task CreateBookCommand_WithoutCover_RetrievalServiceFails_UsesDefaultCover()
    {
        // Arrange — retrieval service always returns the default cover placeholder.
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        var coverRetrievalService = new FakeBookCoverRetrievalService
        {
            ReturnValue = "DefaultBookCoverImage.png"
        };
        var handler = new CreateBookCommandHandler(
            NullLogger<CreateBookCommandHandler>.Instance,
            context,
            cache,
            routeService,
            coverRetrievalService);

        var command = new CreateBookCommand(
            Title: "Domain Driven Design",
            ISBN: "9780321125217",
            Publisher: "Addison Wesley Professional",
            Category: BookCategory.Science,
            Author: "Eric Evans",
            CoverImageFileName: null); // no cover → auto-retrieval triggered

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — command must still succeed even when retrieval returns the default.
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        coverRetrievalService.CallCount.Should().Be(1, because: "retrieval is invoked when CoverImageFileName is null");
        // The route service wraps whatever the retrieval service returned.
        result.Value.BookCoverImageUrl.Should().Contain("DefaultBookCoverImage.png");
    }

    [Fact]
    public async Task CreateBookCommand_WithoutCover_RetrievalServiceReturnsUrl_UrlReachesDto()
    {
        // Arrange — retrieval service returns a real-looking https:// URL.
        const string retrievedUrl = "https://covers.openlibrary.org/b/isbn/9780321125217-L.jpg";

        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        var coverRetrievalService = new FakeBookCoverRetrievalService
        {
            ReturnValue = retrievedUrl
        };
        var handler = new CreateBookCommandHandler(
            NullLogger<CreateBookCommandHandler>.Instance,
            context,
            cache,
            routeService,
            coverRetrievalService);

        var command = new CreateBookCommand(
            Title: "Refactoring",
            ISBN: "9780201485677",
            Publisher: "Addison Wesley Professional",
            Category: BookCategory.Science,
            Author: "Martin Fowler",
            CoverImageFileName: null); // no cover → auto-retrieval triggered

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — URL returned by the retrieval service must appear in the DTO.
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        coverRetrievalService.CallCount.Should().Be(1);
        result.Value.BookCoverImageUrl.Should().Contain(retrievedUrl,
            because: "the URL returned by the retrieval service must be stored and surfaced in the BookDto");
    }
}
