namespace BookOrbit.Application.SubcutaneousTests.Books.Queries;

using BookOrbit.Application.Features.Books.Queries.GetBookById;
using BookOrbit.Application.Features.Books.Queries.GetBooks;
using BookOrbit.Application.SubcutaneousTests.Books.TestDoubles;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.Books.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BookQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetBookByIdQuery_ShouldReturnBook()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var routeService = new FakeRouteService();
        var book = StudentTestFactory.CreateBook(title: "Target Book");
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var handler = new GetBookByIdQueryHandler(
            NullLogger<GetBookByIdQueryHandler>.Instance,
            context,
            routeService);

        // Act
        var result = await handler.Handle(new GetBookByIdQuery(book.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(book.Id);
        result.Value.Title.Should().Be("Target Book");
    }

    [Fact]
    public async Task GetBooksQuery_ShouldFilterAndSort()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var routeService = new FakeRouteService();
        
        var book1 = StudentTestFactory.CreateBook(title: "Science Book", publisherValue: "Pub A");
        var book2 = StudentTestFactory.CreateBook(title: "Math Book", publisherValue: "Pub B");
        
        context.Books.AddRange(book1, book2);
        await context.SaveChangesAsync();

        var handler = new GetBookQueryHandler(context, routeService);
        var query = new GetBooksQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: "Science",
            SortColumn: "title",
            SortDirection: "asc");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items!.Single().Title.Should().Be("Science Book");
    }
}
