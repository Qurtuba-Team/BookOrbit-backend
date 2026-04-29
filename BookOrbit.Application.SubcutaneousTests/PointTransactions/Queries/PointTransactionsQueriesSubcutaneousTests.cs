namespace BookOrbit.Application.SubcutaneousTests.PointTransactions.Queries;

using BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactionById;
using BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactions;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.PointTransactions;
using BookOrbit.Domain.PointTransactions.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class PointTransactionsQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetPointTransactionByIdQuery_ShouldReturnTransaction()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var student = StudentTestFactory.CreateStudent();
        var transaction = CreatePointTransaction(student.Id, null, 5, PointTransactionReason.Reward);

        context.Students.Add(student);
        context.PointTransactions.Add(transaction);
        await context.SaveChangesAsync();

        var handler = new GetPointTransactionByIdQueryHandler(
            NullLogger<GetPointTransactionByIdQueryHandler>.Instance,
            context);

        // Act
        var result = await handler.Handle(new GetPointTransactionByIdQuery(transaction.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(transaction.Id);
        result.Value.StudentId.Should().Be(student.Id);
        result.Value.Points.Should().Be(5);
        result.Value.Reason.Should().Be(PointTransactionReason.Reward);
        result.Value.Direction.Should().Be(PointTransactionDirection.Add);
    }

    [Fact]
    public async Task GetPointTransactionsQuery_ShouldFilterByStudentAndReason()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var student1 = StudentTestFactory.CreateStudent(name: "Student One", userId: "user-1");
        var student2 = StudentTestFactory.CreateStudent(name: "Student Two", userId: "user-2");

        var reviewId = Guid.NewGuid();
        var transaction1 = CreatePointTransaction(student1.Id, null, 5, PointTransactionReason.Reward);
        var transaction2 = CreatePointTransaction(student2.Id, reviewId, 3, PointTransactionReason.GoodReview);

        StudentTestFactory.SetCreatedAt(transaction1, DateTimeOffset.UtcNow.AddMinutes(-5));
        StudentTestFactory.SetCreatedAt(transaction2, DateTimeOffset.UtcNow.AddMinutes(-3));

        context.Students.Add(student1);
        context.Students.Add(student2);
        context.PointTransactions.AddRange(transaction1, transaction2);
        await context.SaveChangesAsync();

        var handler = new GetPointTransactionsQueryHandler(context);

        var query = new GetPointTransactionsQuery(
            Page: 1,
            PageSize: 10,
            SearchTerm: "Student",
            StudentId: student1.Id,
            Reasons: [PointTransactionReason.Reward]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeNull();
        result.Value.Items!.Count.Should().Be(1);
        result.Value.Items.First().StudentId.Should().Be(student1.Id);
        result.Value.Items.First().Reason.Should().Be(PointTransactionReason.Reward);
    }

    private static PointTransaction CreatePointTransaction(
        Guid studentId,
        Guid? borrowingReviewId,
        int points,
        PointTransactionReason reason)
    {
        return PointTransaction.Create(
            Guid.NewGuid(),
            studentId,
            borrowingReviewId,
            points,
            reason).Value;
    }
}
