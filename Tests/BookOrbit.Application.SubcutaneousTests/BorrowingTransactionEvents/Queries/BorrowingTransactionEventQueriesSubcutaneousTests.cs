namespace BookOrbit.Application.SubcutaneousTests.BorrowingTransactionEvents.Queries;

using BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEventById;
using BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEvents;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.BorrowingTransactions.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BorrowingTransactionEventQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetBorrowingTransactionEventByIdQuery_ShouldReturnEvent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-event-1");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-event-1");
        var book = StudentTestFactory.CreateBook(title: "Event Lookup Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var transaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            lender.Id,
            borrower.Id,
            bookCopy.Id,
            now);
        var transactionEvent = StudentTestFactory.CreateBorrowingTransactionEvent(
            transaction.Id,
            BorrowingTransactionState.Borrowed);
        StudentTestFactory.SetCreatedAt(transactionEvent, now.AddMinutes(-2));
        StudentTestFactory.SetLastModifiedAt(transactionEvent, now.AddMinutes(-1));

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.BorrowingTransactions.Add(transaction);
        context.BorrowingTransactionEvents.Add(transactionEvent);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingTransactionEventByIdQueryHandler(
            NullLogger<GetBorrowingTransactionEventByIdQueryHandler>.Instance,
            context);

        // Act
        var result = await handler.Handle(new GetBorrowingTransactionEventByIdQuery(transactionEvent.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(transactionEvent.Id);
        result.Value.BorrowingTransactionId.Should().Be(transaction.Id);
        result.Value.State.Should().Be(BorrowingTransactionState.Borrowed);
        result.Value.CreatedAtUtc.Should().Be(now.AddMinutes(-2));
        result.Value.LastModifiedUtc.Should().Be(now.AddMinutes(-1));
    }

    [Fact]
    public async Task GetBorrowingTransactionEventsQuery_ShouldFilterSearchAndSortEvents()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-event-2");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-event-2");
        var otherLender = StudentTestFactory.CreateStudent(name: "Other Lender", userId: "other-lender-event-1");
        var otherBorrower = StudentTestFactory.CreateStudent(name: "Other Borrower", userId: "other-borrower-event-1");

        var targetBook = StudentTestFactory.CreateBook(title: "Event Book One", isbnValue: "9780306406161");
        var otherBook = StudentTestFactory.CreateBook(title: "Other Event Book", isbnValue: "9780306406162");

        var targetBookCopy = StudentTestFactory.CreateBookCopy(targetBook, lender.Id, BookCopyCondition.New);
        var otherBookCopy = StudentTestFactory.CreateBookCopy(otherBook, otherLender.Id, BookCopyCondition.New);

        var targetTransaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            lender.Id,
            borrower.Id,
            targetBookCopy.Id,
            now.AddDays(-3));
        var otherTransaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            otherLender.Id,
            otherBorrower.Id,
            otherBookCopy.Id,
            now.AddDays(-1));

        var olderEvent = StudentTestFactory.CreateBorrowingTransactionEvent(
            targetTransaction.Id,
            BorrowingTransactionState.Borrowed);
        var newerEvent = StudentTestFactory.CreateBorrowingTransactionEvent(
            targetTransaction.Id,
            BorrowingTransactionState.Borrowed);
        var filteredOutEvent = StudentTestFactory.CreateBorrowingTransactionEvent(
            otherTransaction.Id,
            BorrowingTransactionState.Lost);

        StudentTestFactory.SetCreatedAt(olderEvent, now.AddMinutes(-10));
        StudentTestFactory.SetCreatedAt(newerEvent, now.AddMinutes(-5));
        StudentTestFactory.SetCreatedAt(filteredOutEvent, now.AddMinutes(-1));

        context.Students.AddRange(lender, borrower, otherLender, otherBorrower);
        context.Books.AddRange(targetBook, otherBook);
        context.BookCopies.AddRange(targetBookCopy, otherBookCopy);
        context.BorrowingTransactions.AddRange(targetTransaction, otherTransaction);
        context.BorrowingTransactionEvents.AddRange(olderEvent, newerEvent, filteredOutEvent);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingTransactionEventsQueryHandler(context);
        var query = new GetBorrowingTransactionEventsQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: targetTransaction.Id.ToString(),
            SortColumn: "createdAt",
            SortDirection: "desc",
            States: [BorrowingTransactionState.Borrowed]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeNull();
        result.Value.Items!.Count.Should().Be(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);

        var items = result.Value.Items.ToList();
        items[0].Id.Should().Be(newerEvent.Id);
        items[0].BorrowingTransactionId.Should().Be(targetTransaction.Id);
        items[0].State.Should().Be(BorrowingTransactionState.Borrowed);
        items[1].Id.Should().Be(olderEvent.Id);
        items[1].BorrowingTransactionId.Should().Be(targetTransaction.Id);
    }
}
