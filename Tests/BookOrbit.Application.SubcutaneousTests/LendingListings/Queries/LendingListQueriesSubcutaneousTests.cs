namespace BookOrbit.Application.SubcutaneousTests.LendingListings.Queries;

using BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecordById;
using BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecords;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.LendingListings.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class LendingListQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetLendingListRecordByIdQuery_ShouldReturnRecord()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var owner = StudentTestFactory.CreateStudent(name: "Owner", userId: "owner-3");
        var book = StudentTestFactory.CreateBook(title: "Query Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id, BookCopyCondition.New);
        var record = StudentTestFactory.CreateLendingListRecord(bookCopy, now);

        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(record);
        await context.SaveChangesAsync();

        var handler = new GetLendingListRecordByIdQueryHandler(
            NullLogger<GetLendingListRecordByIdQueryHandler>.Instance,
            context);

        // Act
        var result = await handler.Handle(new GetLendingListRecordByIdQuery(record.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(record.Id);
        result.Value.BookCopyId.Should().Be(bookCopy.Id);
        result.Value.BorrowingDurationInDays.Should().Be(record.BorrowingDurationInDays);
    }

    [Fact]
    public async Task GetLendingListRecordsQuery_ShouldFilterByBookAndState()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var owner = StudentTestFactory.CreateStudent(name: "Owner", userId: "owner-4");
        var book = StudentTestFactory.CreateBook(title: "Search Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id, BookCopyCondition.New);
        var record = StudentTestFactory.CreateLendingListRecord(bookCopy, now);

        StudentTestFactory.SetCreatedAt(record, now.AddMinutes(-2));
        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(bookCopy, "Owner", owner);
        StudentTestFactory.SetNavigation(record, "BookCopy", bookCopy);

        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(record);
        await context.SaveChangesAsync();

        var handler = new GetLendingListRecordsQueryHandler(context);

        var query = new GetLendingListRecordsQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: "Search",
            BookId: book.Id,
            States: [LendingListRecordState.Available]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeNull();
        result.Value.Items!.Count.Should().Be(1);
        result.Value.Items.First().BookCopyId.Should().Be(bookCopy.Id);
        result.Value.Items.First().OwnerId.Should().Be(owner.Id);
    }
}
