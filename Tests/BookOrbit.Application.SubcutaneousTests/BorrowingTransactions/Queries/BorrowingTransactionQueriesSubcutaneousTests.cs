namespace BookOrbit.Application.SubcutaneousTests.BorrowingTransactions.Queries;

using BookOrbit.Application.Features.BorrowingTransactions.Queries.GetBorrowingTransactionById;
using BookOrbit.Application.Features.BorrowingTransactions.Queries.GetBorrowingTransactions;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.BorrowingTransactions.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BorrowingTransactionQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetBorrowingTransactionByIdQuery_ShouldReturnTransaction()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-4");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-4");
        var book = StudentTestFactory.CreateBook(title: "Lookup Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var transaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            lender.Id,
            borrower.Id,
            bookCopy.Id,
            now);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.BorrowingTransactions.Add(transaction);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingTransactionByIdQueryHandler(
            NullLogger<GetBorrowingTransactionByIdQueryHandler>.Instance,
            context);

        // Act
        var result = await handler.Handle(new GetBorrowingTransactionByIdQuery(transaction.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(transaction.Id);
        result.Value.BorrowingRequestId.Should().Be(transaction.BorrowingRequestId);
        result.Value.LenderStudentId.Should().Be(lender.Id);
        result.Value.BorrowerStudentId.Should().Be(borrower.Id);
        result.Value.BookCopyId.Should().Be(bookCopy.Id);
        result.Value.State.Should().Be(BorrowingTransactionState.Borrowed);
    }

    [Fact]
    public async Task GetBorrowingTransactionsQuery_ShouldFilterAndSortTransactions()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Target Lender", userId: "lender-5");
        var borrower = StudentTestFactory.CreateStudent(name: "Target Borrower", userId: "borrower-5");
        var otherLender = StudentTestFactory.CreateStudent(name: "Other Lender", userId: "other-1");
        var otherBorrower = StudentTestFactory.CreateStudent(name: "Other Borrower", userId: "other-2");

        var olderBook = StudentTestFactory.CreateBook(title: "Target Book One", isbnValue: "9780306406158");
        var newerBook = StudentTestFactory.CreateBook(title: "Target Book Two", isbnValue: "9780306406159");
        var filteredOutBook = StudentTestFactory.CreateBook(title: "Filtered Out", isbnValue: "9780306406160");

        var olderBookCopy = StudentTestFactory.CreateBookCopy(olderBook, lender.Id, BookCopyCondition.New);
        var newerBookCopy = StudentTestFactory.CreateBookCopy(newerBook, lender.Id, BookCopyCondition.New);
        var filteredOutBookCopy = StudentTestFactory.CreateBookCopy(filteredOutBook, otherLender.Id, BookCopyCondition.New);

        var olderTransaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            lender.Id,
            borrower.Id,
            olderBookCopy.Id,
            now.AddDays(-3));
        var newerTransaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            lender.Id,
            borrower.Id,
            newerBookCopy.Id,
            now.AddDays(-2));
        var filteredOutTransaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            otherLender.Id,
            otherBorrower.Id,
            filteredOutBookCopy.Id,
            now.AddDays(-1));

        StudentTestFactory.SetCreatedAt(olderTransaction, now.AddMinutes(-10));
        StudentTestFactory.SetCreatedAt(newerTransaction, now.AddMinutes(-5));
        StudentTestFactory.SetCreatedAt(filteredOutTransaction, now.AddMinutes(-1));

        StudentTestFactory.SetNavigation(olderBookCopy, "Book", olderBook);
        StudentTestFactory.SetNavigation(newerBookCopy, "Book", newerBook);
        StudentTestFactory.SetNavigation(filteredOutBookCopy, "Book", filteredOutBook);

        StudentTestFactory.SetNavigation(olderTransaction, "LenderStudent", lender);
        StudentTestFactory.SetNavigation(olderTransaction, "BorrowerStudent", borrower);
        StudentTestFactory.SetNavigation(olderTransaction, "BookCopy", olderBookCopy);

        StudentTestFactory.SetNavigation(newerTransaction, "LenderStudent", lender);
        StudentTestFactory.SetNavigation(newerTransaction, "BorrowerStudent", borrower);
        StudentTestFactory.SetNavigation(newerTransaction, "BookCopy", newerBookCopy);

        StudentTestFactory.SetNavigation(filteredOutTransaction, "LenderStudent", otherLender);
        StudentTestFactory.SetNavigation(filteredOutTransaction, "BorrowerStudent", otherBorrower);
        StudentTestFactory.SetNavigation(filteredOutTransaction, "BookCopy", filteredOutBookCopy);

        context.Students.AddRange(lender, borrower, otherLender, otherBorrower);
        context.Books.AddRange(olderBook, newerBook, filteredOutBook);
        context.BookCopies.AddRange(olderBookCopy, newerBookCopy, filteredOutBookCopy);
        context.BorrowingTransactions.AddRange(olderTransaction, newerTransaction, filteredOutTransaction);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingTransactionsQueryHandler(context);
        var query = new GetBorrowingTransactionsQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: "Target",
            SortColumn: "createdAt",
            SortDirection: "desc",
            BorrowerStudentId: borrower.Id,
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
        items[0].Id.Should().Be(newerTransaction.Id);
        items[0].BookTitle.Should().Be("Target Book Two");
        items[0].BorrowerStudentName.Should().Be("Target Borrower");
        items[1].Id.Should().Be(olderTransaction.Id);
    }

    [Fact]
    public async Task GetBorrowingTransactionByIdQuery_ShouldReturnError_WhenNotFound()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var handler = new GetBorrowingTransactionByIdQueryHandler(
            NullLogger<GetBorrowingTransactionByIdQueryHandler>.Instance,
            context);

        // Act
        var result = await handler.Handle(new GetBorrowingTransactionByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BorrowingTransaction.BorrowingTransaction.NotFound");
    }

    [Fact]
    public async Task GetBorrowingTransactionsQuery_ShouldFilterByLenderStudentId()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender One", userId: "lender-filter-1");
        var otherLender = StudentTestFactory.CreateStudent(name: "Lender Two", userId: "lender-filter-2");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-filter");

        var book = StudentTestFactory.CreateBook(title: "Filter Book");
        var bookCopy1 = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var bookCopy2 = StudentTestFactory.CreateBookCopy(book, otherLender.Id, BookCopyCondition.New);

        var transaction1 = StudentTestFactory.CreateBorrowingTransaction(Guid.NewGuid(), lender.Id, borrower.Id, bookCopy1.Id, now.AddDays(7));
        var transaction2 = StudentTestFactory.CreateBorrowingTransaction(Guid.NewGuid(), otherLender.Id, borrower.Id, bookCopy2.Id, now.AddDays(7));

        StudentTestFactory.SetNavigation(bookCopy1, "Book", book);
        StudentTestFactory.SetNavigation(bookCopy2, "Book", book);
        StudentTestFactory.SetNavigation(transaction1, "LenderStudent", lender);
        StudentTestFactory.SetNavigation(transaction1, "BorrowerStudent", borrower);
        StudentTestFactory.SetNavigation(transaction1, "BookCopy", bookCopy1);
        StudentTestFactory.SetNavigation(transaction2, "LenderStudent", otherLender);
        StudentTestFactory.SetNavigation(transaction2, "BorrowerStudent", borrower);
        StudentTestFactory.SetNavigation(transaction2, "BookCopy", bookCopy2);

        context.Students.AddRange(lender, otherLender, borrower);
        context.Books.Add(book);
        context.BookCopies.AddRange(bookCopy1, bookCopy2);
        context.BorrowingTransactions.AddRange(transaction1, transaction2);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingTransactionsQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetBorrowingTransactionsQuery(Page: 1, PageSize: 10, SearchTerm: null, LenderStudentId: lender.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items!.Single().LenderStudentId.Should().Be(lender.Id);
    }
}
