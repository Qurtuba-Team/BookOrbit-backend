using BookOrbit.Application.UnitTests.Helpers;
using BookOrbit.Domain.Common.Entities;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace BookOrbit.Application.UnitTests.Recommendations.Commands;

public class RecordInteractionCommandHandlerTests
{
    private readonly HybridCache _cache;
    private readonly ILogger<RecordInteractionCommandHandler> _logger;

    public RecordInteractionCommandHandlerTests()
    {
        _cache  = A.Fake<HybridCache>();
        _logger = A.Fake<ILogger<RecordInteractionCommandHandler>>();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="IAppDbContext"/> whose UserBookInteractions DbSet
    /// is backed by the supplied list and is queryable via async LINQ.
    /// </summary>
    private IAppDbContext BuildContext(List<UserBookInteraction> seed)
    {
        var context  = A.Fake<IAppDbContext>();
        var fakeDbSet = MockDbSetHelper.CreateDbSet(seed);
        A.CallTo(() => context.UserBookInteractions).Returns(fakeDbSet);
        A.CallTo(() => context.SaveChangesAsync(A<CancellationToken>._)).Returns(1);
        return context;
    }

    private RecordInteractionCommandHandler CreateSut(IAppDbContext context) =>
        new(context, _cache, _logger);

    // ── Happy paths ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenNoExistingInteraction_CreatesNewEntry()
    {
        // Arrange
        var context = BuildContext([]);   // empty — no existing interaction

        var command = new RecordInteractionCommand(
            UserId:       "user-1",
            BookId:       Guid.NewGuid(),
            Rating:       4,
            IsRead:       true,
            IsWishlisted: false);

        // Act
        var result = await CreateSut(context).Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        A.CallTo(() => context.UserBookInteractions.Add(A<UserBookInteraction>.That.Matches(i =>
            i.UserId       == command.UserId        &&
            i.BookId       == command.BookId        &&
            i.Rating       == command.Rating        &&
            i.IsRead       == command.IsRead        &&
            i.IsWishlisted == command.IsWishlisted)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => context.SaveChangesAsync(A<CancellationToken>._))
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
            Id           = Guid.NewGuid(),
            UserId       = userId,
            BookId       = bookId,
            Rating       = 2,
            IsRead       = false,
            IsWishlisted = false
        };

        var context = BuildContext([existing]);

        var command = new RecordInteractionCommand(
            UserId:       userId,
            BookId:       bookId,
            Rating:       5,
            IsRead:       true,
            IsWishlisted: true);

        // Act
        var result = await CreateSut(context).Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        existing.Rating.Should().Be(5);
        existing.IsRead.Should().BeTrue();
        existing.IsWishlisted.Should().BeTrue();

        A.CallTo(() => context.UserBookInteractions.Add(A<UserBookInteraction>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Always_InvalidatesUserScopedCache()
    {
        // Arrange
        const string userId     = "user-cache-check";
        var expectedCacheKey    = $"recommendations:{userId}";

        var context = BuildContext([]);

        var command = new RecordInteractionCommand(
            UserId:       userId,
            BookId:       Guid.NewGuid(),
            Rating:       null,
            IsRead:       true,
            IsWishlisted: false);

        // Act
        await CreateSut(context).Handle(command, CancellationToken.None);

        // Assert — cache removal called with the user-scoped key
        A.CallTo(() => _cache.RemoveAsync(expectedCacheKey, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WithNullRating_AndWishlistTrue_StoresCorrectly()
    {
        // Arrange
        var context = BuildContext([]);

        var command = new RecordInteractionCommand(
            UserId:       "user-wish",
            BookId:       Guid.NewGuid(),
            Rating:       null,
            IsRead:       false,
            IsWishlisted: true);

        // Act
        var result = await CreateSut(context).Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => context.UserBookInteractions.Add(A<UserBookInteraction>.That.Matches(i =>
            i.Rating       == null  &&
            i.IsWishlisted == true  &&
            i.IsRead       == false)))
            .MustHaveHappenedOnceExactly();
    }

    // ── SaveChanges is always called ─────────────────────────────────────────

    [Fact]
    public async Task Handle_Always_CallsSaveChangesAsync()
    {
        // Arrange
        var context = BuildContext([]);
        var command = new RecordInteractionCommand("u", Guid.NewGuid(), null, false, false);

        // Act
        await CreateSut(context).Handle(command, CancellationToken.None);

        // Assert
        A.CallTo(() => context.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
