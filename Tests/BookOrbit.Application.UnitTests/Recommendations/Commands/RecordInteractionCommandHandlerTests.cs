using BookOrbit.Application.UnitTests.Helpers;
using BookOrbit.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace BookOrbit.Application.UnitTests.Recommendations.Commands;

public class RecordInteractionCommandHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly HybridCache _cache;
    private readonly ILogger<RecordInteractionCommandHandler> _logger;

    public RecordInteractionCommandHandlerTests()
    {
        _context = A.Fake<IAppDbContext>();
        _cache   = A.Fake<HybridCache>();
        _logger  = A.Fake<ILogger<RecordInteractionCommandHandler>>();
    }

    private RecordInteractionCommandHandler CreateSut() =>
        new(_context, _cache, _logger);

    // ── Happy paths ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenNoExistingInteraction_CreatesNewEntry()
    {
        // Arrange
        var command = new RecordInteractionCommand(
            UserId:       "user-1",
            BookId:       Guid.NewGuid(),
            Rating:       4,
            IsRead:       true,
            IsWishlisted: false);

        A.CallTo(() => _context.UserBookInteractions)
            .Returns(MockDbSetHelper.CreateDbSet(new List<UserBookInteraction>())); // empty — no existing

        A.CallTo(() => _context.SaveChangesAsync(A<CancellationToken>._)).Returns(1);

        // Act
        var result = await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        A.CallTo(() => _context.UserBookInteractions.Add(A<UserBookInteraction>.That.Matches(i =>
            i.UserId       == command.UserId  &&
            i.BookId       == command.BookId  &&
            i.Rating       == command.Rating  &&
            i.IsRead       == command.IsRead  &&
            i.IsWishlisted == command.IsWishlisted)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _context.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenExistingInteraction_UpdatesItInPlace_WithoutAdd()
    {
        // Arrange
        const string userId = "user-2";
        var bookId          = Guid.NewGuid();

        var existing = new UserBookInteraction
        {
            UserId       = userId,
            BookId       = bookId,
            Rating       = 2,
            IsRead       = false,
            IsWishlisted = false
        };

        A.CallTo(() => _context.UserBookInteractions)
            .Returns(MockDbSetHelper.CreateDbSet(new List<UserBookInteraction> { existing }));

        A.CallTo(() => _context.SaveChangesAsync(A<CancellationToken>._)).Returns(1);

        var command = new RecordInteractionCommand(
            UserId:       userId,
            BookId:       bookId,
            Rating:       5,
            IsRead:       true,
            IsWishlisted: true);

        // Act
        var result = await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        existing.Rating.Should().Be(5);
        existing.IsRead.Should().BeTrue();
        existing.IsWishlisted.Should().BeTrue();

        A.CallTo(() => _context.UserBookInteractions.Add(A<UserBookInteraction>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Always_InvalidatesUserScopedCache()
    {
        // Arrange
        const string userId = "user-cache-check";
        var expectedKey     = $"recommendations:{userId}";

        A.CallTo(() => _context.UserBookInteractions)
            .Returns(MockDbSetHelper.CreateDbSet(new List<UserBookInteraction>()));

        A.CallTo(() => _context.SaveChangesAsync(A<CancellationToken>._)).Returns(1);

        var command = new RecordInteractionCommand(
            UserId:       userId,
            BookId:       Guid.NewGuid(),
            Rating:       null,
            IsRead:       true,
            IsWishlisted: false);

        // Act
        await CreateSut().Handle(command, CancellationToken.None);

        // Assert — cache removal called with the user-scoped key
        A.CallTo(() => _cache.RemoveAsync(expectedKey, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WithNullRating_AndWishlistTrue_StoresCorrectly()
    {
        // Arrange
        A.CallTo(() => _context.UserBookInteractions)
            .Returns(MockDbSetHelper.CreateDbSet(new List<UserBookInteraction>()));
        A.CallTo(() => _context.SaveChangesAsync(A<CancellationToken>._)).Returns(1);

        var command = new RecordInteractionCommand(
            UserId:       "user-wish",
            BookId:       Guid.NewGuid(),
            Rating:       null,
            IsRead:       false,
            IsWishlisted: true);

        // Act
        var result = await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _context.UserBookInteractions.Add(A<UserBookInteraction>.That.Matches(i =>
            i.Rating       == null  &&
            i.IsWishlisted == true  &&
            i.IsRead       == false)))
            .MustHaveHappenedOnceExactly();
    }
}
