namespace BookOrbit.Application.SubcutaneousTests.BorrowingReviews.Queries;

using BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviewById;
using BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviews;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BorrowingReviewQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetBorrowingReviewByIdQuery_ShouldReturnReview()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "review-lender-2");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "review-borrower-2");
        var book = StudentTestFactory.CreateBook(title: "Lookup Review Book", isbnValue: "9780306406165");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, lender.Id, BookCopyCondition.New);
        var transaction = StudentTestFactory.CreateBorrowingTransaction(
            Guid.NewGuid(),
            lender.Id,
            borrower.Id,
            bookCopy.Id,
            now);
        var review = StudentTestFactory.CreateBorrowingReview(
            reviewerStudentId: borrower.Id,
            reviewedStudentId: lender.Id,
            borrowingTransactionId: transaction.Id,
            rating: 4,
            description: "Smooth borrowing process.");

        context.Students.AddRange(lender, borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.BorrowingTransactions.Add(transaction);
        context.BorrowingReviews.Add(review);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingReviewByIdQueryHandler(
            NullLogger<GetBorrowingReviewByIdQueryHandler>.Instance,
            context);

        // Act
        var result = await handler.Handle(new GetBorrowingReviewByIdQuery(review.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(review.Id);
        result.Value.ReviewerStudentId.Should().Be(borrower.Id);
        result.Value.ReviewedStudentId.Should().Be(lender.Id);
        result.Value.BorrowingTransactionId.Should().Be(transaction.Id);
        result.Value.Description.Should().Be("Smooth borrowing process.");
        result.Value.Rating.Should().Be(4);
    }

    [Fact]
    public async Task GetBorrowingReviewsQuery_ShouldFilterSearchAndSortReviews()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "review-lender-3");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "review-borrower-3");
        var otherLender = StudentTestFactory.CreateStudent(name: "Other Lender", userId: "review-other-lender-1");
        var otherBorrower = StudentTestFactory.CreateStudent(name: "Other Borrower", userId: "review-other-borrower-1");

        var targetBook = StudentTestFactory.CreateBook(title: "Target Review Book", isbnValue: "9780306406166");
        var otherBook = StudentTestFactory.CreateBook(title: "Other Review Book", isbnValue: "9780306406167");

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
            now.AddDays(-2));

        var olderReview = StudentTestFactory.CreateBorrowingReview(
            reviewerStudentId: borrower.Id,
            reviewedStudentId: lender.Id,
            borrowingTransactionId: targetTransaction.Id,
            rating: 3,
            description: "Helpful and flexible lender");
        var newerReview = StudentTestFactory.CreateBorrowingReview(
            reviewerStudentId: borrower.Id,
            reviewedStudentId: lender.Id,
            borrowingTransactionId: targetTransaction.Id,
            rating: 5,
            description: "Helpful follow up after return");
        var filteredOutReview = StudentTestFactory.CreateBorrowingReview(
            reviewerStudentId: otherBorrower.Id,
            reviewedStudentId: otherLender.Id,
            borrowingTransactionId: otherTransaction.Id,
            rating: 1,
            description: "Not the target review");

        StudentTestFactory.SetCreatedAt(olderReview, now.AddMinutes(-10));
        StudentTestFactory.SetCreatedAt(newerReview, now.AddMinutes(-5));
        StudentTestFactory.SetCreatedAt(filteredOutReview, now.AddMinutes(-1));

        context.Students.AddRange(lender, borrower, otherLender, otherBorrower);
        context.Books.AddRange(targetBook, otherBook);
        context.BookCopies.AddRange(targetBookCopy, otherBookCopy);
        context.BorrowingTransactions.AddRange(targetTransaction, otherTransaction);
        context.BorrowingReviews.AddRange(olderReview, newerReview, filteredOutReview);
        await context.SaveChangesAsync();

        var handler = new GetBorrowingReviewsQueryHandler(context);
        var query = new GetBorrowingReviewsQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: "Helpful",
            SortColumn: "rating",
            SortDirection: "desc",
            ReviewerStudentId: borrower.Id,
            ReviewedStudentId: lender.Id,
            BorrowingTransactionId: targetTransaction.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeNull();
        result.Value.Items!.Count.Should().Be(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);

        var items = result.Value.Items.ToList();
        items[0].Id.Should().Be(newerReview.Id);
        items[0].Rating.Should().Be(5);
        items[0].Description.Should().Be("Helpful follow up after return");
        items[1].Id.Should().Be(olderReview.Id);
        items[1].Rating.Should().Be(3);
    }
}
