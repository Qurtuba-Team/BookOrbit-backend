namespace BookOrbit.Application.SubcutaneousTests.BorrowingTransactions.Commands;

using BookOrbit.Application.Features.BorrowingTransactions.Commands.CreateBorrowingTransaction;
using BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsLostBorrowingTransaction;
using BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsReturnedBorrowingTransaction;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.BorrowingRequests.Enums;
using BookOrbit.Domain.BorrowingTransactions;
using BookOrbit.Domain.BorrowingTransactions.Enums;
using BookOrbit.Domain.LendingListings.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BorrowingTransactionCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateBorrowingTransactionCommand_ShouldPersistTransactionAndUpdateRelatedEntities()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-1");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-1");
        var book = StudentTestFactory.CreateBook(title: "Borrowed Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        var borrowingRequest = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord.Id, now);

        lendingRecord.MarkAsReserved();
        borrowingRequest.MarkAsApproved();

        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(bookCopy, "Owner", lender);
        StudentTestFactory.SetNavigation(lendingRecord, "BookCopy", bookCopy);
        StudentTestFactory.SetNavigation(borrowingRequest, "BorrowingStudent", borrower);
        StudentTestFactory.SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.Add(borrowingRequest);
        await context.SaveChangesAsync();

        var handler = new CreateBorrowingTransactionCommandHandler(
            NullLogger<CreateBorrowingTransactionCommandHandler>.Instance,
            context,
            TimeProvider.System,
            cache);

        // Act
        var result = await handler.Handle(new CreateBorrowingTransactionCommand(borrowingRequest.Id), CancellationToken.None);

        // Assert
        result.Errors.Select(error => error.Code).Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.BorrowingRequestId.Should().Be(borrowingRequest.Id);
        result.Value.LenderStudentId.Should().Be(lender.Id);
        result.Value.BorrowerStudentId.Should().Be(borrower.Id);
        result.Value.BookCopyId.Should().Be(bookCopy.Id);
        result.Value.State.Should().Be(BorrowingTransactionState.Borrowed);

        context.BorrowingTransactions.Should().HaveCount(1);
        context.BorrowingTransactionEvents.Should().HaveCount(1);
        context.BorrowingTransactions.Single().State.Should().Be(BorrowingTransactionState.Borrowed);
        context.BorrowingTransactionEvents.Single().State.Should().Be(BorrowingTransactionState.Borrowed);
        bookCopy.State.Should().Be(BookCopyState.Borrowed);
        lendingRecord.State.Should().Be(LendingListRecordState.Borrowed);
        borrowingRequest.State.Should().Be(BorrowingRequestState.Accepted);
    }

    [Fact]
    public async Task MarkAsReturnedBorrowingTransactionCommand_ShouldReturnTransactionAndMakeBookCopyAvailable()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow.AddDays(-2);

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-2");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-2");
        var book = StudentTestFactory.CreateBook(title: "Returnable Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var transaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            lender.Id,
            borrower.Id,
            bookCopy.Id,
            now);

        bookCopy.MarkAsBorrowed();
        StudentTestFactory.SetCreatedAt(transaction, now.AddMinutes(-5));
        StudentTestFactory.SetNavigation(transaction, "BookCopy", bookCopy);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.BorrowingTransactions.Add(transaction);
        await context.SaveChangesAsync();

        var handler = new MarkAsReturnedBorrowingTransactionCommandHandler(
            context,
            TimeProvider.System,
            NullLogger<MarkAsReturnedBorrowingTransactionCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new MarkAsReturnedBorrowingTransactionCommand(transaction.Id), CancellationToken.None);

        // Assert
        result.Errors.Select(error => error.Code).Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Returned);
        transaction.ActualReturnDate.Should().NotBeNull();
        bookCopy.State.Should().Be(BookCopyState.Available);
        context.BorrowingTransactionEvents.Should().HaveCount(1);
        context.BorrowingTransactionEvents.Single().State.Should().Be(BorrowingTransactionState.Returned);
    }

    [Fact]
    public async Task MarkAsLostBorrowingTransactionCommand_ShouldMarkTransactionAndBookCopyAsLost()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow.AddDays(-1);

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-3");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-3");
        var book = StudentTestFactory.CreateBook(title: "Lost Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var transaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            lender.Id,
            borrower.Id,
            bookCopy.Id,
            now);

        bookCopy.MarkAsBorrowed();
        StudentTestFactory.SetNavigation(transaction, "BookCopy", bookCopy);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.BorrowingTransactions.Add(transaction);
        await context.SaveChangesAsync();

        var handler = new MarkAsLostBorrowingTransactionCommandHandler(
            context,
            NullLogger<MarkAsLostBorrowingTransactionCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(new MarkAsLostBorrowingTransactionCommand(transaction.Id), CancellationToken.None);

        // Assert
        result.Errors.Select(error => error.Code).Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Lost);
        bookCopy.State.Should().Be(BookCopyState.Lost);
        context.BorrowingTransactionEvents.Should().HaveCount(1);
        context.BorrowingTransactionEvents.Single().State.Should().Be(BorrowingTransactionState.Lost);
    }

    [Fact]
    public async Task CreateBorrowingTransactionCommand_ShouldReturnError_WhenBorrowingRequestNotFound()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var handler = new CreateBorrowingTransactionCommandHandler(
            NullLogger<CreateBorrowingTransactionCommandHandler>.Instance,
            context,
            TimeProvider.System,
            cache);

        // Act
        var result = await handler.Handle(new CreateBorrowingTransactionCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BorrowingRequest.BorrowingRequest.NotFound");
    }

    [Fact]
    public async Task CreateBorrowingTransactionCommand_ShouldReturnError_WhenBorrowingRequestNotAccepted()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-fail");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-fail");
        var book = StudentTestFactory.CreateBook(title: "Failed Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        var borrowingRequest = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord.Id, now);

        // Borrowing request is still Pending, not Accepted
        StudentTestFactory.SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);
        StudentTestFactory.SetNavigation(lendingRecord, "BookCopy", bookCopy);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.Add(borrowingRequest);
        await context.SaveChangesAsync();

        var handler = new CreateBorrowingTransactionCommandHandler(
            NullLogger<CreateBorrowingTransactionCommandHandler>.Instance,
            context,
            TimeProvider.System,
            cache);

        // Act
        var result = await handler.Handle(new CreateBorrowingTransactionCommand(borrowingRequest.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BorrowingRequest.BorrowingRequestNotAccepted");
    }

    [Fact]
    public async Task MarkAsReturnedBorrowingTransactionCommand_ShouldMarkAsOverdue_WhenReturnedAfterExpectedDate()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;
        var expectedReturnDate = now.AddDays(-1);

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-overdue");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-overdue");
        var book = StudentTestFactory.CreateBook(title: "Overdue Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        
        // Manual creation to control ExpectedReturnDate exactly
        var transactionResult = BorrowingTransaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(), // borrowingRequestId
            lender.Id,
            borrower.Id,
            bookCopy.Id,
            expectedReturnDate,
            expectedReturnDate.AddDays(-5));
            
        var transaction = transactionResult.Value;

        bookCopy.MarkAsBorrowed();
        StudentTestFactory.SetCreatedAt(transaction, expectedReturnDate.AddDays(-5));
        StudentTestFactory.SetNavigation(transaction, "BookCopy", bookCopy);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.BorrowingTransactions.Add(transaction);
        await context.SaveChangesAsync();

        var handler = new MarkAsReturnedBorrowingTransactionCommandHandler(
            context,
            TimeProvider.System,
            NullLogger<MarkAsReturnedBorrowingTransactionCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new MarkAsReturnedBorrowingTransactionCommand(transaction.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.State.Should().Be(BorrowingTransactionState.Overdue);
        transaction.ActualReturnDate.Should().NotBeNull();
        bookCopy.State.Should().Be(BookCopyState.Available);
    }

    [Fact]
    public async Task MarkAsReturnedBorrowingTransactionCommand_ShouldReturnError_WhenTransactionNotFound()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var handler = new MarkAsReturnedBorrowingTransactionCommandHandler(
            context,
            TimeProvider.System,
            NullLogger<MarkAsReturnedBorrowingTransactionCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new MarkAsReturnedBorrowingTransactionCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BorrowingTransaction.BorrowingTransaction.NotFound");
    }
}
