namespace BookOrbit.Application.SubcutaneousTests.BookCopies.Queries;

using BookOrbit.Application.Features.BookCopies.Queries.GetBookCopies;
using BookOrbit.Application.Features.BookCopies.Queries.GetBookCopyById;
using BookOrbit.Application.SubcutaneousTests.Books.TestDoubles;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BookCopyQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetBookCopyByIdQuery_ShouldReturnBookCopy()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var routeService = new FakeRouteService();
        var book = StudentTestFactory.CreateBook();
        var owner = StudentTestFactory.CreateStudent();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id);

        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(bookCopy, "Owner", owner);

        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        await context.SaveChangesAsync();

        var handler = new GetBookCopyByIdQueryHandler(
            NullLogger<GetBookCopyByIdQueryHandler>.Instance,
            context,
            routeService);

        // Act
        var result = await handler.Handle(new GetBookCopyByIdQuery(bookCopy.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(bookCopy.Id);
        result.Value.Title.Should().Be(book.Title.Value);
        result.Value.OwnerName.Should().Be(owner.Name.Value);
    }

    [Fact]
    public async Task GetBookCopiesQuery_ShouldFilterAndSort()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var routeService = new FakeRouteService();
        var book = StudentTestFactory.CreateBook(title: "Shared Book");
        var owner1 = StudentTestFactory.CreateStudent(name: "Owner One", userId: "owner-1");
        var owner2 = StudentTestFactory.CreateStudent(name: "Owner Two", userId: "owner-2");

        var copy1 = StudentTestFactory.CreateBookCopy(book, owner1.Id, BookCopyCondition.New);
        var copy2 = StudentTestFactory.CreateBookCopy(book, owner2.Id, BookCopyCondition.Poor);

        StudentTestFactory.SetNavigation(copy1, "Book", book);
        StudentTestFactory.SetNavigation(copy1, "Owner", owner1);
        StudentTestFactory.SetNavigation(copy2, "Book", book);
        StudentTestFactory.SetNavigation(copy2, "Owner", owner2);

        context.Students.AddRange(owner1, owner2);
        context.Books.Add(book);
        context.BookCopies.AddRange(copy1, copy2);
        await context.SaveChangesAsync();

        var handler = new GetBookCopiesQueryHandler(context, routeService);
        var query = new GetBookCopiesQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: "Owner One",
            SortColumn: "ownername",
            SortDirection: "asc");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items!.Single().OwnerName.Should().Be("Owner One");
    }
}
