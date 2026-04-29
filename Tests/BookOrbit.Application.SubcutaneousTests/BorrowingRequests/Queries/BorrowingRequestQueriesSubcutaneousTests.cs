namespace BookOrbit.Application.SubcutaneousTests.BorrowingRequests.Queries;

using BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequestById;
using BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequests;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.BorrowingRequests.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BorrowingRequestQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetBorrowingRequestByIdQuery_ShouldReturnRequest()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "lender-q1");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-q1");
        var book = StudentTestFactory.CreateBook(title: "Requested Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        var borrowingRequest = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord.Id, now);

        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(lendingRecord, "BookCopy", bookCopy);
        StudentTestFactory.SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);
        StudentTestFactory.SetNavigation(borrowingRequest, "BorrowingStudent", borrower);

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.Add(borrowingRequest);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingRequestByIdQueryHandler(NullLogger<GetBorrowingRequestByIdQueryHandler>.Instance, context);

        // Act
        var result = await handler.Handle(new GetBorrowingRequestByIdQuery(borrowingRequest.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(borrowingRequest.Id);
        result.Value.BorrowingStudentId.Should().Be(borrower.Id);
        result.Value.LendingRecordId.Should().Be(lendingRecord.Id);
    }

    [Fact]
    public async Task GetBorrowingRequestsQuery_ShouldFilterAndSort()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender Two", userId: "lender-q2");
        var borrower = StudentTestFactory.CreateStudent(name: "Target Borrower", userId: "borrower-q2");
        var otherBorrower = StudentTestFactory.CreateStudent(name: "Other Borrower", userId: "other-q2");

        var book1 = StudentTestFactory.CreateBook(title: "Book One");
        var book2 = StudentTestFactory.CreateBook(title: "Book Two");

        var bookCopy1 = StudentTestFactory.CreateBookCopy(book1, lender.Id, BookCopyCondition.New);
        var bookCopy2 = StudentTestFactory.CreateBookCopy(book2, lender.Id, BookCopyCondition.New);

        var lendingRecord1 = StudentTestFactory.CreateLendingListRecord(bookCopy1, now);
        var lendingRecord2 = StudentTestFactory.CreateLendingListRecord(bookCopy2, now);

        var request1 = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord1.Id, now.AddDays(-1));
        var request2 = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord2.Id, now);
        var request3 = StudentTestFactory.CreateBorrowingRequest(otherBorrower.Id, lendingRecord1.Id, now);

        StudentTestFactory.SetCreatedAt(request1, now.AddMinutes(-10));
        StudentTestFactory.SetCreatedAt(request2, now.AddMinutes(-5));

        StudentTestFactory.SetNavigation(bookCopy1, "Book", book1);
        StudentTestFactory.SetNavigation(bookCopy2, "Book", book2);
        StudentTestFactory.SetNavigation(lendingRecord1, "BookCopy", bookCopy1);
        StudentTestFactory.SetNavigation(lendingRecord2, "BookCopy", bookCopy2);
        StudentTestFactory.SetNavigation(request1, "BorrowingStudent", borrower);
        StudentTestFactory.SetNavigation(request1, "LendingRecord", lendingRecord1);
        StudentTestFactory.SetNavigation(request2, "BorrowingStudent", borrower);
        StudentTestFactory.SetNavigation(request2, "LendingRecord", lendingRecord2);
        StudentTestFactory.SetNavigation(request3, "BorrowingStudent", otherBorrower);
        StudentTestFactory.SetNavigation(request3, "LendingRecord", lendingRecord1);

        context.Students.AddRange(lender, borrower, otherBorrower);
        context.Books.AddRange(book1, book2);
        context.BookCopies.AddRange(bookCopy1, bookCopy2);
        context.LendingListRecords.AddRange(lendingRecord1, lendingRecord2);
        context.BorrowingRequests.AddRange(request1, request2, request3);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingRequestsQueryHandler(context);
        var query = new GetBorrowingRequestsQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: "Book",
            SortColumn: "createdAt",
            SortDirection: "desc",
            BorrowingStudentId: borrower.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items!.First().Id.Should().Be(request2.Id);
    }
}
