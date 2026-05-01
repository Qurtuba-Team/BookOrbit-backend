using BookOrbit.Application.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookOrbit.Application.UnitTests.Recommendations.Queries;

public class GetTrendingBooksQueryHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly ILogger<GetTrendingBooksQueryHandler> _logger;

    public GetTrendingBooksQueryHandlerTests()
    {
        _context = A.Fake<IAppDbContext>();
        _logger  = A.Fake<ILogger<GetTrendingBooksQueryHandler>>();
    }

    private GetTrendingBooksQueryHandler CreateSut() =>
        new(_context, _logger);

    // ── Happy paths ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithBooksAndRatings_ReturnsTopRatedBooksOrderedDescending()
    {
        // Arrange
        var bookHigh   = BookFactory.CreateValid(title: "Book High",   category: BookCategory.Science);
        var bookMedium = BookFactory.CreateValid(title: "Book Medium",  category: BookCategory.Fiction);
        var bookLow    = BookFactory.CreateValid(title: "Book Low",     category: BookCategory.Mystery);
        var books      = new List<Book> { bookHigh, bookMedium, bookLow };

        var interactions = new List<UserBookInteraction>
        {
            new() { BookId = bookHigh.Id,   Rating = 5,    UserId = "u1" },
            new() { BookId = bookHigh.Id,   Rating = 4,    UserId = "u2" },   // avg 4.5
            new() { BookId = bookMedium.Id, Rating = 3,    UserId = "u1" },   // avg 3.0
            new() { BookId = bookLow.Id,    Rating = 2,    UserId = "u1" },   // avg 2.0
        };

        A.CallTo(() => _context.Books).Returns(MockDbSetHelper.CreateDbSet(books));
        A.CallTo(() => _context.UserBookInteractions).Returns(MockDbSetHelper.CreateDbSet(interactions));

        // Act
        var result = await CreateSut().Handle(new GetTrendingBooksQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value[0].Title.Should().Be("Book High");
        result.Value[0].Score.Should().BeApproximately(4.5, 0.001);
        result.Value.Should().BeInDescendingOrder(r => r.Score);
    }

    [Fact]
    public async Task Handle_WithNoBooksInCatalog_ReturnsEmptySuccessResult()
    {
        // Arrange
        A.CallTo(() => _context.Books).Returns(MockDbSetHelper.CreateDbSet(new List<Book>()));
        A.CallTo(() => _context.UserBookInteractions).Returns(MockDbSetHelper.CreateDbSet(new List<UserBookInteraction>()));

        // Act
        var result = await CreateSut().Handle(new GetTrendingBooksQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithBooksButNoRatings_ReturnsAllBooksWithScoreZero()
    {
        // Arrange
        var book1 = BookFactory.CreateValid(title: "Unrated A");
        var book2 = BookFactory.CreateValid(title: "Unrated B");

        A.CallTo(() => _context.Books).Returns(MockDbSetHelper.CreateDbSet(new List<Book> { book1, book2 }));
        A.CallTo(() => _context.UserBookInteractions).Returns(MockDbSetHelper.CreateDbSet(new List<UserBookInteraction>()));

        // Act
        var result = await CreateSut().Handle(new GetTrendingBooksQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(r => r.Score.Should().Be(0.0));
    }

    [Fact]
    public async Task Handle_WithTiedAverageRatings_OrdersByRatingsCountDescending()
    {
        // Arrange
        var bookMoreRatings  = BookFactory.CreateValid(title: "Popular");
        var bookFewerRatings = BookFactory.CreateValid(title: "Niche");

        var interactions = new List<UserBookInteraction>
        {
            // both avg 4.0, but Popular has 3 ratings vs Niche's 1
            new() { BookId = bookMoreRatings.Id,  Rating = 4, UserId = "u1" },
            new() { BookId = bookMoreRatings.Id,  Rating = 4, UserId = "u2" },
            new() { BookId = bookMoreRatings.Id,  Rating = 4, UserId = "u3" },
            new() { BookId = bookFewerRatings.Id, Rating = 4, UserId = "u1" },
        };

        A.CallTo(() => _context.Books)
            .Returns(MockDbSetHelper.CreateDbSet(new List<Book> { bookMoreRatings, bookFewerRatings }));
        A.CallTo(() => _context.UserBookInteractions)
            .Returns(MockDbSetHelper.CreateDbSet(interactions));

        // Act
        var result = await CreateSut().Handle(new GetTrendingBooksQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Title.Should().Be("Popular");
        result.Value[1].Title.Should().Be("Niche");
    }

    [Fact]
    public async Task Handle_ReasonLabel_IsAlwaysTrending()
    {
        // Arrange
        var book = BookFactory.CreateValid();
        A.CallTo(() => _context.Books).Returns(MockDbSetHelper.CreateDbSet(new List<Book> { book }));
        A.CallTo(() => _context.UserBookInteractions).Returns(MockDbSetHelper.CreateDbSet(new List<UserBookInteraction>()));

        // Act
        var result = await CreateSut().Handle(new GetTrendingBooksQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().AllSatisfy(r => r.ReasonLabel.Should().Be("Trending"));
    }

    // ── ICachedQuery contract ────────────────────────────────────────────────

    [Fact]
    public void Query_CacheKey_IsSharedTrendingKey()
    {
        var query = new GetTrendingBooksQuery();
        query.CacheKey.Should().Be(RecommendationsCachingConstants.TrendingKey);
    }

    [Fact]
    public void Query_Expiration_Is24Hours()
    {
        var query = new GetTrendingBooksQuery();
        query.Expiration.Should().Be(TimeSpan.FromMinutes(RecommendationsCachingConstants.TrendingExpirationInMinutes));
    }
}
