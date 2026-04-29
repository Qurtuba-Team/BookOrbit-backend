namespace BookOrbit.Application.SubcutaneousTests.BorrowingReviews.Commands;

using BookOrbit.Application.Features.BorrowingReviews.Commands.CreateBorrowingReview;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class BorrowingReviewCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateBorrowingReviewCommand_ShouldPersistReview()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var lender = StudentTestFactory.CreateStudent(name: "Lender", userId: "review-lender-1");
        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "review-borrower-1");
        var book = StudentTestFactory.CreateBook(title: "Reviewed Book", isbnValue: "9780306406164");
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

        var handler = new CreateBorrowingReviewCommandHandler(
            NullLogger<CreateBorrowingReviewCommandHandler>.Instance,
            context,
            cache);

        var command = new CreateBorrowingReviewCommand(
            ReviewerStudentId: borrower.Id,
            ReviewedStudentId: lender.Id,
            BorrowingTransactionId: transaction.Id,
            Description: "Great communication and easy return.",
            Rating: 5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReviewerStudentId.Should().Be(borrower.Id);
        result.Value.ReviewedStudentId.Should().Be(lender.Id);
        result.Value.BorrowingTransactionId.Should().Be(transaction.Id);
        result.Value.Description.Should().Be("Great communication and easy return.");
        result.Value.Rating.Should().Be(5);

        context.BorrowingReviews.Should().HaveCount(1);
        var review = context.BorrowingReviews.Single();
        review.ReviewerStudentId.Should().Be(borrower.Id);
        review.ReviewedStudentId.Should().Be(lender.Id);
        review.BorrowingTransactionId.Should().Be(transaction.Id);
        review.Description.Should().Be("Great communication and easy return.");
        review.Rating.Value.Should().Be(5);
    }
}
