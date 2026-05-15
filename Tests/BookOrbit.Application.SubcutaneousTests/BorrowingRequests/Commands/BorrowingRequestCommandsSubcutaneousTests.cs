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
using BookOrbit.Infrastructure.Services.ConcurrencyServices;
using BookOrbit.Infrastructure.Common.Errors;
using BookOrbit.Domain.Common.Results;
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
        borrower.Points.Value.Should().Be(Point.StudentInitialPoint.Value + 10 - lendingRecord.Cost.Value);
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
        yield return new object[] { nameof(AcceptBorrowingRequestCommandHandler), CreateAcceptBorrowingRequestExecution(null) };
        yield return new object[] { nameof(RejectBorrowingRequestCommandHandler), CreateRejectBorrowingRequestExecution(null) };
        yield return new object[] { nameof(CancelBorrowingRequestCommandHandler), CreateCancelBorrowingRequestExecution(null) };
        yield return new object[] { nameof(ExpireBorrowingRequestCommandHandler), CreateExpireBorrowingRequestExecution(null) };
    }

    public static IEnumerable<object[]> InvalidConcurrencyFormatScenarios()
    {
        const string invalidRowVersion = "not-a-base64-token";

        yield return new object[] { nameof(AcceptBorrowingRequestCommandHandler), CreateAcceptBorrowingRequestExecution(invalidRowVersion) };
        yield return new object[] { nameof(RejectBorrowingRequestCommandHandler), CreateRejectBorrowingRequestExecution(invalidRowVersion) };
        yield return new object[] { nameof(CancelBorrowingRequestCommandHandler), CreateCancelBorrowingRequestExecution(invalidRowVersion) };
        yield return new object[] { nameof(ExpireBorrowingRequestCommandHandler), CreateExpireBorrowingRequestExecution(invalidRowVersion) };
    }

    public static IEnumerable<object[]> ConcurrencyConflictScenarios()
    {
        string conflictRowVersion = Convert.ToBase64String(new byte[] { 9, 9, 9 });

        yield return new object[] { nameof(AcceptBorrowingRequestCommandHandler), CreateAcceptBorrowingRequestExecution(conflictRowVersion) };
        yield return new object[] { nameof(RejectBorrowingRequestCommandHandler), CreateRejectBorrowingRequestExecution(conflictRowVersion) };
        yield return new object[] { nameof(CancelBorrowingRequestCommandHandler), CreateCancelBorrowingRequestExecution(conflictRowVersion) };
        yield return new object[] { nameof(ExpireBorrowingRequestCommandHandler), CreateExpireBorrowingRequestExecution(conflictRowVersion) };
    }

    private static Func<Task<Result<Updated>>> CreateAcceptBorrowingRequestExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var now = DateTimeOffset.UtcNow;

            var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-acc");
            var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-acc");
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

            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);

            var handler = new AcceptBorrowingRequestCommandHandler(context, concurrencyService, NullLogger<AcceptBorrowingRequestCommandHandler>.Instance, cache);

            return await handler.Handle(new AcceptBorrowingRequestCommand(borrowingRequest.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateRejectBorrowingRequestExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var now = DateTimeOffset.UtcNow;

            var borrower = StudentTestFactory.CreateStudent(name: "Borrower Reject", userId: "borrower-rej");
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

            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);

            var handler = new RejectBorrowingRequestCommandHandler(context, concurrencyService, NullLogger<RejectBorrowingRequestCommandHandler>.Instance, cache);

            return await handler.Handle(new RejectBorrowingRequestCommand(borrowingRequest.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateCancelBorrowingRequestExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var now = DateTimeOffset.UtcNow;

            var borrower = StudentTestFactory.CreateStudent(name: "Borrower Cancel", userId: "borrower-canc");
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

            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);

            var handler = new CancelBorrowingRequestCommandHandler(context, concurrencyService, NullLogger<CancelBorrowingRequestCommandHandler>.Instance, cache);

            return await handler.Handle(new CancelBorrowingRequestCommand(borrowingRequest.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateExpireBorrowingRequestExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var now = DateTimeOffset.UtcNow;

            var borrower = StudentTestFactory.CreateStudent(name: "Borrower Expire", userId: "borrower-exp");
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

            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);

            var handler = new ExpireBorrowingRequestCommandHandler(context, concurrencyService, NullLogger<ExpireBorrowingRequestCommandHandler>.Instance, cache);

            return await handler.Handle(new ExpireBorrowingRequestCommand(borrowingRequest.Id, rowVersion!), CancellationToken.None);
        };

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

        var concurrencyService = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new AcceptBorrowingRequestCommandHandler(context, concurrencyService, NullLogger<AcceptBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new AcceptBorrowingRequestCommand(borrowingRequest.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

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

        var concurrencyService2 = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new RejectBorrowingRequestCommandHandler(context, concurrencyService2, NullLogger<RejectBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new RejectBorrowingRequestCommand(borrowingRequest.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        borrowingRequest.State.Should().Be(BorrowingRequestState.Rejected);
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

        var concurrencyService3 = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new CancelBorrowingRequestCommandHandler(context, concurrencyService3, NullLogger<CancelBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new CancelBorrowingRequestCommand(borrowingRequest.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        borrowingRequest.State.Should().Be(BorrowingRequestState.Cancelled);
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

        var concurrencyService4 = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new ExpireBorrowingRequestCommandHandler(context, concurrencyService4, NullLogger<ExpireBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new ExpireBorrowingRequestCommand(borrowingRequest.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        borrowingRequest.State.Should().Be(BorrowingRequestState.Expired);
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

        var concurrencyServiceFail = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new AcceptBorrowingRequestCommandHandler(context, concurrencyServiceFail, NullLogger<AcceptBorrowingRequestCommandHandler>.Instance, cache);

        // Act
        var result = await handler.Handle(new AcceptBorrowingRequestCommand(request2.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BorrowingRequest.LendingRecordNotAvailable");
    }
}
