namespace BookOrbit.Application.SubcutaneousTests.LendingListings.Commands;

using BookOrbit.Application.Features.LendingListings.Commands.CreateLendingListRecord;
using BookOrbit.Application.Features.LendingListings.Commands.StateMachien.CloseLendingListRecord;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.LendingListings;
using BookOrbit.Domain.LendingListings.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class LendingListCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateLendingListRecordCommand_ShouldPersistRecord()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var owner = StudentTestFactory.CreateStudent(name: "Owner", userId: "owner-1");
        var book = StudentTestFactory.CreateBook(title: "Lending Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id, BookCopyCondition.New);

        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        await context.SaveChangesAsync();

        var handler = new CreateLendingListRecordCommandHandler(
            NullLogger<CreateLendingListRecordCommandHandler>.Instance,
            context,
            TimeProvider.System,
            cache);

        var command = new CreateLendingListRecordCommand(
            BookCopyId: bookCopy.Id,
            BorrowingDurationInDays: LendingListRecord.MinBorrowingDurationInDays);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BookCopyId.Should().Be(bookCopy.Id);
        result.Value.State.Should().Be(LendingListRecordState.Available);
        context.LendingListRecords.Should().HaveCount(1);
        context.LendingListRecords.Single().BookCopyId.Should().Be(bookCopy.Id);
    }

    [Fact]
    public async Task CloseLendingListRecordCommand_ShouldCloseRecord()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var now = DateTimeOffset.UtcNow;

        var owner = StudentTestFactory.CreateStudent(name: "Owner", userId: "owner-2");
        var book = StudentTestFactory.CreateBook(title: "Close Book");
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id, BookCopyCondition.New);
        var record = StudentTestFactory.CreateLendingListRecord(bookCopy, now);

        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(record);
        await context.SaveChangesAsync();

        var handler = new CloseLendingListRecordCommandHandler(
            context,
            NullLogger<CloseLendingListRecordCommandHandler>.Instance,
            cache);

        var command = new CloseLendingListRecordCommand(record.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.State.Should().Be(LendingListRecordState.Closed);
    }
}
