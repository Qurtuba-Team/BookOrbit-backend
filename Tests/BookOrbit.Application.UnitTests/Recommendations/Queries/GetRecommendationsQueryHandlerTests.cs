using BookOrbit.Application.UnitTests.Helpers;
using Microsoft.Extensions.Logging;

namespace BookOrbit.Application.UnitTests.Recommendations.Queries;

public class GetRecommendationsQueryHandlerTests
{
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<GetRecommendationsQueryHandler> _logger;
    private readonly GetRecommendationsQueryHandler _sut;

    public GetRecommendationsQueryHandlerTests()
    {
        _recommendationService = A.Fake<IRecommendationService>();
        _logger                = A.Fake<ILogger<GetRecommendationsQueryHandler>>();
        _sut                   = new GetRecommendationsQueryHandler(_recommendationService, _logger);
    }

    // ── Happy paths ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithValidUserId_DelegatesToRecommendationService()
    {
        // Arrange
        const string userId = "user-abc";
        var expected = new List<RecommendationDto>
        {
            new(Guid.NewGuid(), "Clean Code", "Robert Martin", "Nonfiction", "Intermediate", "Based on your interests", 4.5)
        };

        A.CallTo(() => _recommendationService.GetRecommendationsAsync(userId, A<CancellationToken>._))
            .Returns(expected);

        var query = new GetRecommendationsQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);

        A.CallTo(() => _recommendationService.GetRecommendationsAsync(userId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsEmptyList_ReturnsEmptySuccess()
    {
        // Arrange
        const string userId = "user-no-history";
        A.CallTo(() => _recommendationService.GetRecommendationsAsync(userId, A<CancellationToken>._))
            .Returns(new List<RecommendationDto>());

        var query = new GetRecommendationsQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMultipleBooks_ReturnsAllRecommendations()
    {
        // Arrange
        const string userId = "user-xyz";
        var expected = Enumerable.Range(1, 5).Select(i =>
            new RecommendationDto(Guid.NewGuid(), $"Book {i}", "Author", "Fiction", "Beginner", "Trending", i * 0.5))
            .ToList();

        A.CallTo(() => _recommendationService.GetRecommendationsAsync(userId, A<CancellationToken>._))
            .Returns(expected);

        var query = new GetRecommendationsQuery(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    // ── CacheKey / ICachedQuery contract ────────────────────────────────────

    [Fact]
    public void Query_CacheKey_IsUserScoped()
    {
        const string userId = "user-123";
        var query = new GetRecommendationsQuery(userId);

        query.CacheKey.Should().Be(RecommendationsCachingConstants.UserRecommendationsKey(userId));
    }

    [Fact]
    public void Query_Tags_ContainsRecommendationsTag()
    {
        var query = new GetRecommendationsQuery("any-user");
        query.Tags.Should().Contain(RecommendationsCachingConstants.RecommendationsTag);
    }

    [Fact]
    public void Query_Expiration_Is6Hours()
    {
        var query = new GetRecommendationsQuery("any-user");
        query.Expiration.Should().Be(TimeSpan.FromMinutes(RecommendationsCachingConstants.RecommendationsExpirationInMinutes));
    }
}
