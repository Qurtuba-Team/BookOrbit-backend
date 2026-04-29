namespace BookOrbit.Application.SubcutaneousTests.BorrowingRequests.Commands;

using BookOrbit.Application.Features.BorrowingRequests;
using BookOrbit.Application.Features.BorrowingRequests.Commands.CreateBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.CancelBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.ExpireBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.RejectBorrowingRequest;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.BorrowingRequests.Enums;
using BookOrbit.Domain.LendingListings.Enums;
using BookOrbit.Domain.PointTransactions.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BorrowingRequestCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateBorrowingRequestCommand_ShouldPersistRequestAndDeductPoints()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-1");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-1");
        borrower.AddPoints(Point.Create(10).Value); // Give enough points

        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);

        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(lendingRecord, "BookCopy", bookCopy);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        await context.SaveChangesAsync();

        var handler = new CreateBorrowingRequestCommandHandler(
            NullLogger<CreateBorrowingRequestCommandHandler>.Instance,
            context,
            TimeProvider.System,
            cache);

        var command = new CreateBorrowingRequestCommand(borrower.Id, lendingRecord.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BorrowingStudentId.Should().Be(borrower.Id);
        result.Value.LendingRecordId.Should().Be(lendingRecord.Id);
        result.Value.State.Should().Be(BorrowingRequestState.Pending);

        context.BorrowingRequests.Should().HaveCount(1);
        borrower.Points.Value.Should().Be(1 + 10 - 5); // Default start 1 + 10 added - 5 cost
        context.PointTransactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateBorrowingRequestCommand_ShouldReturnError_WhenLendingRecordNotAvailable()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-2");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-2");
        borrower.AddPoints(Point.Create(10).Value);

        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        lendingRecord.MarkAsReserved(); // Make it unavailable

        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(lendingRecord, "BookCopy", bookCopy);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        await context.SaveChangesAsync();

        var handler = new CreateBorrowingRequestCommandHandler(
            NullLogger<CreateBorrowingRequestCommandHandler>.Instance,
            context,
            TimeProvider.System,
            cache);

        // Act
        var result = await handler.Handle(new CreateBorrowingRequestCommand(borrower.Id, lendingRecord.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BorrowingRequest.LendingRecordNotAvailable");
    }

    [Fact]
    public async Task AcceptBorrowingRequestCommand_ShouldUpdateStateAndReserveLendingRecord()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-3");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-3");
        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        var borrowingRequest = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord.Id, now);

        StudentTestFactory.SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.Add(borrowingRequest);
        await context.SaveChangesAsync();

        var handler = new AcceptBorrowingRequestCommandHandler(context, NullLogger<AcceptBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new AcceptBorrowingRequestCommand(borrowingRequest.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        borrowingRequest.State.Should().Be(BorrowingRequestState.Accepted);
        lendingRecord.State.Should().Be(LendingListRecordState.Reserved);
    }

    [Fact]
    public async Task RejectBorrowingRequestCommand_ShouldUpdateState()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var borrower = StudentTestFactory.CreateStudent(name: "Borrower Four", userId: "borrower-4");
        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, Guid.NewGuid(), BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        var borrowingRequest = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord.Id, now);

        StudentTestFactory.SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);
        StudentTestFactory.SetNavigation(borrowingRequest, "BorrowingStudent", borrower);

        context.Students.Add(borrower);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.Add(borrowingRequest);
        await context.SaveChangesAsync();

        var handler = new RejectBorrowingRequestCommandHandler(context, NullLogger<RejectBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new RejectBorrowingRequestCommand(borrowingRequest.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        borrowingRequest.State.Should().Be(BorrowingRequestState.Rejected);
        borrower.Points.Value.Should().Be(1 + 5);
    }

    [Fact]
    public async Task CancelBorrowingRequestCommand_ShouldUpdateStateAndReturnPoints()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var borrower = StudentTestFactory.CreateStudent(name: "Borrower Five", userId: "borrower-5");
        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, Guid.NewGuid(), BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        var borrowingRequest = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord.Id, now);

        StudentTestFactory.SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);
        StudentTestFactory.SetNavigation(borrowingRequest, "BorrowingStudent", borrower);

        context.Students.Add(borrower);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.Add(borrowingRequest);
        await context.SaveChangesAsync();

        var handler = new CancelBorrowingRequestCommandHandler(context, NullLogger<CancelBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new CancelBorrowingRequestCommand(borrowingRequest.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        borrowingRequest.State.Should().Be(BorrowingRequestState.Cancelled);
        borrower.Points.Value.Should().Be(1 + 5); // Start 1 + 5 returned
    }

    [Fact]
    public async Task ExpireBorrowingRequestCommand_ShouldUpdateStateAndReturnPoints()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var borrower = StudentTestFactory.CreateStudent(name: "Borrower Six", userId: "borrower-6");
        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, Guid.NewGuid(), BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        var borrowingRequest = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord.Id, now);

        StudentTestFactory.SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);
        StudentTestFactory.SetNavigation(borrowingRequest, "BorrowingStudent", borrower);

        context.Students.Add(borrower);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.Add(borrowingRequest);
        await context.SaveChangesAsync();

        var handler = new ExpireBorrowingRequestCommandHandler(context, NullLogger<ExpireBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new ExpireBorrowingRequestCommand(borrowingRequest.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        borrowingRequest.State.Should().Be(BorrowingRequestState.Expired);
        borrower.Points.Value.Should().Be(1 + 5);
    }

    [Fact]
    public async Task CreateBorrowingRequestCommand_ShouldReturnError_WhenStudentRequestsOwnCopy()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-fail");
        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);

        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(lendingRecord, "BookCopy", bookCopy);

        context.Students.Add(lender);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        await context.SaveChangesAsync();

        var handler = new CreateBorrowingRequestCommandHandler(
            NullLogger<CreateBorrowingRequestCommandHandler>.Instance,
            context,
            TimeProvider.System,
            cache);

        // Act
        var result = await handler.Handle(new CreateBorrowingRequestCommand(lender.Id, lendingRecord.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BorrowingRequest.StudentCannotBorrowOwnedCopies");
    }

    [Fact]
    public async Task CreateBorrowingRequestCommand_ShouldReturnError_WhenStudentHasInsufficientPoints()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-fail-2");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower Poor", userId: "borrower-fail-2");
        // borrower starts with 1 point, cost is 5

        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);

        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(lendingRecord, "BookCopy", bookCopy);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        await context.SaveChangesAsync();

        var handler = new CreateBorrowingRequestCommandHandler(
            NullLogger<CreateBorrowingRequestCommandHandler>.Instance,
            context,
            TimeProvider.System,
            cache);

        // Act
        var result = await handler.Handle(new CreateBorrowingRequestCommand(borrower.Id, lendingRecord.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Student.InsufficientPoints");
    }

    [Fact]
    public async Task AcceptBorrowingRequestCommand_ShouldReturnError_WhenAnotherRequestAlreadyAccepted()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-3-fail");
        var borrower1 = StudentTestFactory.CreateStudent(name: "Borrower One", userId: "borrower-3-f1");
        var borrower2 = StudentTestFactory.CreateStudent(name: "Borrower Two", userId: "borrower-3-f2");
        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        
        var request1 = StudentTestFactory.CreateBorrowingRequest(borrower1.Id, lendingRecord.Id, now);
        request1.MarkAsApproved(); // Already accepted
        
        var request2 = StudentTestFactory.CreateBorrowingRequest(borrower2.Id, lendingRecord.Id, now);

        StudentTestFactory.SetNavigation(request2, "LendingRecord", lendingRecord);

        context.Students.AddRange(lender, borrower1, borrower2);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.AddRange(request1, request2);
        await context.SaveChangesAsync();

        var handler = new AcceptBorrowingRequestCommandHandler(context, NullLogger<AcceptBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new AcceptBorrowingRequestCommand(request2.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BorrowingRequest.LendingRecordNotAvailable");
    }
}
