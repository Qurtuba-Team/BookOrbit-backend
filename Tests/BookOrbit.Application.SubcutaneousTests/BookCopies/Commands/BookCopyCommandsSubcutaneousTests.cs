namespace BookOrbit.Application.SubcutaneousTests.BookCopies.Commands;

using BookOrbit.Application.Features.BookCopies.Commands.CreateBookCopy;
using BookOrbit.Application.Features.BookCopies.Commands.UpdateBookCopy;
using BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeAvilableBookCopy;
using BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeUnAvilableBookCopy;
using BookOrbit.Application.SubcutaneousTests.Books.TestDoubles;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.Books.Enums;
using BookOrbit.Domain.Students.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using BookOrbit.Infrastructure.Services.ConcurrencyServices;
using BookOrbit.Infrastructure.Common.Errors;
using BookOrbit.Domain.Common.Results;
using Xunit;

public class BookCopyCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateBookCopyCommand_ShouldPersistBookCopy()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        
        var book = StudentTestFactory.CreateBook();
        book.MarkAsAvailable();
        var owner = StudentTestFactory.CreateStudent();
        owner.MarkAsApproved(DateTimeOffset.UtcNow);
        owner.MarkAsActivated();

        context.Books.Add(book);
        context.Students.Add(owner);
        await context.SaveChangesAsync();

        var handler = new CreateBookCopyCommandHandler(
            NullLogger<CreateBookCopyCommandHandler>.Instance,
            context,
            cache,
            routeService);

        var command = new CreateBookCopyCommand(owner.Id, book.Id, BookCopyCondition.LikeNew);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OwnerId.Should().Be(owner.Id);
        result.Value.BookId.Should().Be(book.Id);
        result.Value.Condition.Should().Be(BookCopyCondition.LikeNew);
        result.Value.State.Should().Be(BookCopyState.Available);

        context.BookCopies.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateBookCopyCommand_ShouldUpdateCondition()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook();
        var owner = StudentTestFactory.CreateStudent();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id, BookCopyCondition.New);
        
        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        await context.SaveChangesAsync();

        var concurrencyService = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new UpdateBookCopyCommandHandler(
            NullLogger<UpdateBookCopyCommandHandler>.Instance,
            context,
            concurrencyService,
            cache);

        var command = new UpdateBookCopyCommand(bookCopy.Id, BookCopyCondition.Acceptable, Convert.ToBase64String(new byte[] {1,2,3}));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.Condition.Should().Be(BookCopyCondition.Acceptable);
    }

    [Fact]
    public async Task CreateBookCopyCommand_ShouldReturnError_WhenBookNotAvailable()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var routeService = new FakeRouteService();
        
        var book = StudentTestFactory.CreateBook();
        // Default status is Pending
        var owner = StudentTestFactory.CreateStudent();
        owner.MarkAsApproved(DateTimeOffset.UtcNow);
        owner.MarkAsActivated();

        context.Books.Add(book);
        context.Students.Add(owner);
        await context.SaveChangesAsync();

        var handler = new CreateBookCopyCommandHandler(
            NullLogger<CreateBookCopyCommandHandler>.Instance,
            context,
            cache,
            routeService);

        // Act
        var result = await handler.Handle(new CreateBookCopyCommand(owner.Id, book.Id, BookCopyCondition.New), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Book.BookIsNotAvailable");
    }

    [Fact]
    public async Task MakeUnAvilableBookCopyCommand_ShouldUpdateState()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook();
        var owner = StudentTestFactory.CreateStudent();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id);
        // Default state is Available
        
        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        await context.SaveChangesAsync();

        var concurrencyService = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new MakeUnAvilableBookCopyCommandHandler(
            context,
            concurrencyService,
            NullLogger<MakeUnAvilableBookCopyCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new MakeUnAvilableBookCopyCommand(bookCopy.Id, Convert.ToBase64String(new byte[] {1,2,3})), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        bookCopy.State.Should().Be(BookCopyState.UnAvailable);
    }

    [Fact]
    public async Task MakeUnAvilableBookCopyCommand_ShouldReturnError_WhenInUse()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var book = StudentTestFactory.CreateBook();
        var owner = StudentTestFactory.CreateStudent();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, DateTimeOffset.UtcNow);
        
        context.Students.Add(owner);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        await context.SaveChangesAsync();

        var concurrencyService = new BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService(context, NullLogger<BookOrbit.Infrastructure.Services.ConcurrencyServices.ConcurrencyService>.Instance);

        var handler = new MakeUnAvilableBookCopyCommandHandler(
            context,
            concurrencyService,
            NullLogger<MakeUnAvilableBookCopyCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new MakeUnAvilableBookCopyCommand(bookCopy.Id, Convert.ToBase64String(new byte[] {1,2,3})), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BookCopy.BookCopyInUse");
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
        yield return new object[] { nameof(MakeAvilableBookCopyCommandHandler), CreateMakeAvilableExecution(null) };
        yield return new object[] { nameof(MakeUnAvilableBookCopyCommandHandler), CreateMakeUnAvilableExecution(null) };
        yield return new object[] { nameof(UpdateBookCopyCommandHandler), CreateUpdateBookCopyExecution(null) };
    }

    public static IEnumerable<object[]> InvalidConcurrencyFormatScenarios()
    {
        const string invalidRowVersion = "not-a-base64-token";

        yield return new object[] { nameof(MakeAvilableBookCopyCommandHandler), CreateMakeAvilableExecution(invalidRowVersion) };
        yield return new object[] { nameof(MakeUnAvilableBookCopyCommandHandler), CreateMakeUnAvilableExecution(invalidRowVersion) };
        yield return new object[] { nameof(UpdateBookCopyCommandHandler), CreateUpdateBookCopyExecution(invalidRowVersion) };
    }

    public static IEnumerable<object[]> ConcurrencyConflictScenarios()
    {
        string conflictRowVersion = Convert.ToBase64String(new byte[] { 9, 9, 9 });

        yield return new object[] { nameof(MakeAvilableBookCopyCommandHandler), CreateMakeAvilableExecution(conflictRowVersion) };
        yield return new object[] { nameof(MakeUnAvilableBookCopyCommandHandler), CreateMakeUnAvilableExecution(conflictRowVersion) };
        yield return new object[] { nameof(UpdateBookCopyCommandHandler), CreateUpdateBookCopyExecution(conflictRowVersion) };
    }

    private static Func<Task<Result<Updated>>> CreateMakeAvilableExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var book = StudentTestFactory.CreateBook();
            var owner = StudentTestFactory.CreateStudent();
            var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id);
            bookCopy.MarkAsUnAvilable();

            context.Students.Add(owner);
            context.Books.Add(book);
            context.BookCopies.Add(bookCopy);
            await context.SaveChangesAsync();

            var handler = new MakeAvilableBookCopyCommandHandler(
                context,
                concurrencyService,
                NullLogger<MakeAvilableBookCopyCommandHandler>.Instance,
                cache);

            return await handler.Handle(new MakeAvilableBookCopyCommand(bookCopy.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateMakeUnAvilableExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var book = StudentTestFactory.CreateBook();
            var owner = StudentTestFactory.CreateStudent();
            var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id);

            context.Students.Add(owner);
            context.Books.Add(book);
            context.BookCopies.Add(bookCopy);
            await context.SaveChangesAsync();

            var handler = new MakeUnAvilableBookCopyCommandHandler(
                context,
                concurrencyService,
                NullLogger<MakeUnAvilableBookCopyCommandHandler>.Instance,
                cache);

            return await handler.Handle(new MakeUnAvilableBookCopyCommand(bookCopy.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateUpdateBookCopyExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var book = StudentTestFactory.CreateBook();
            var owner = StudentTestFactory.CreateStudent();
            var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id, BookCopyCondition.New);

            context.Students.Add(owner);
            context.Books.Add(book);
            context.BookCopies.Add(bookCopy);
            await context.SaveChangesAsync();

            var handler = new UpdateBookCopyCommandHandler(
                NullLogger<UpdateBookCopyCommandHandler>.Instance,
                context,
                concurrencyService,
                cache);

            return await handler.Handle(new UpdateBookCopyCommand(bookCopy.Id, BookCopyCondition.Acceptable, rowVersion!), CancellationToken.None);
        };
}
