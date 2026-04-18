using BookOrbit.Application.Common.Enums;
using BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;
using BookOrbit.Domain.Books.Enums;
using BookOrbit.Domain.Students.Enums;

namespace BookOrbit.Infrastructure.Services;

public class RecommendationService(
    IAppDbContext context,
    HybridCache cache,
    UserManager<AppUser> userManager,
    ILogger<RecommendationService> logger) : IRecommendationService
{
    private const string RecommendationsCacheKeyPrefix = "recommendations:";
    private const string TrendingCacheKey = "recommendations:trending";
    private const int MinRecommendations = 5;
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(6),
        LocalCacheExpiration = TimeSpan.FromHours(6)
    };
    private static readonly HybridCacheEntryOptions TrendingCacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(24),
        LocalCacheExpiration = TimeSpan.FromHours(24)
    };

    // Maps InterestType → one or more BookCategory flags
    private static readonly Dictionary<InterestType, BookCategory> InterestToCategoryMap = new()
    {
        [InterestType.Fiction]        = BookCategory.Fiction,
        [InterestType.NonFiction]     = BookCategory.Nonfiction,
        [InterestType.Science]        = BookCategory.Science | BookCategory.ScienceFiction,
        [InterestType.Technology]     = BookCategory.Science | BookCategory.Nonfiction,
        [InterestType.History]        = BookCategory.HistoricalFiction | BookCategory.Biography | BookCategory.Autobiography,
        [InterestType.Philosophy]     = BookCategory.Philosophy,
        [InterestType.Medicine]       = BookCategory.Science,
        [InterestType.Law]            = BookCategory.Nonfiction,
        [InterestType.Art]            = BookCategory.Nonfiction,
        [InterestType.SelfDevelopment]= BookCategory.SelfHelp,
        [InterestType.Psychology]     = BookCategory.Psychology,
        [InterestType.Economics]      = BookCategory.Business,
        [InterestType.Literature]     = BookCategory.Fiction | BookCategory.HistoricalFiction,
        [InterestType.Mathematics]    = BookCategory.Science,
        [InterestType.Religion]       = BookCategory.ReligionAndSpirituality,
    };

    // Faculty → category boost
    private static readonly Dictionary<Faculty, BookCategory> FacultyBoostMap = new()
    {
        [Faculty.Medicine]        = BookCategory.Science,
        [Faculty.Pharmacy]        = BookCategory.Science,
        [Faculty.Dentistry]       = BookCategory.Science,
        [Faculty.Nursing]         = BookCategory.Science,
        [Faculty.Engineering]     = BookCategory.Science | BookCategory.Nonfiction,
        [Faculty.ComputerScience] = BookCategory.Science | BookCategory.Nonfiction,
        [Faculty.Science]         = BookCategory.Science,
        [Faculty.Commerce]        = BookCategory.Business,
        [Faculty.Law]             = BookCategory.Nonfiction | BookCategory.HistoricalFiction,
        [Faculty.Arts]            = BookCategory.Fiction | BookCategory.Philosophy | BookCategory.HistoricalFiction,
        [Faculty.Other]           = BookCategory.None,
    };

    public async Task<Result<List<RecommendationDto>>> GetRecommendationsAsync(string userId, CancellationToken ct)
    {
        var cacheKey = $"{RecommendationsCacheKeyPrefix}{userId}";

        var result = await cache.GetOrCreateAsync(
            cacheKey,
            async cancel => await BuildRecommendationsAsync(userId, cancel),
            CacheOptions,
            cancellationToken: ct);

        return result ?? [];
    }

    public async Task<Result<List<RecommendationDto>>> GetTrendingBooksAsync(CancellationToken ct)
    {
        var result = await cache.GetOrCreateAsync(
            TrendingCacheKey,
            async cancel =>
            {
                var trending = await context.Books
                    .Select(b => new
                    {
                        Book = b,
                        AvgRating = context.UserBookInteractions
                            .Where(i => i.BookId == b.Id && i.Rating.HasValue)
                            .Select(i => (double?)i.Rating)
                            .Average() ?? 0.0,
                        RatingsCount = context.UserBookInteractions
                            .Count(i => i.BookId == b.Id && i.Rating.HasValue)
                    })
                    .OrderByDescending(x => x.AvgRating)
                    .ThenByDescending(x => x.RatingsCount)
                    .Take(20)
                    .ToListAsync(cancel);

                return trending.Select(x => new RecommendationDto(
                    BookId: x.Book.Id,
                    Title: x.Book.Title.Value,
                    Author: x.Book.Author.Value,
                    Genre: x.Book.Category.ToString(),
                    Level: string.Empty,
                    ReasonLabel: "Trending",
                    Score: x.AvgRating))
                    .ToList();
            },
            TrendingCacheOptions,
            cancellationToken: ct);

        return result ?? [];
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task<List<RecommendationDto>> BuildRecommendationsAsync(string userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            logger.LogWarning("BuildRecommendations: user {UserId} not found — returning trending fallback.", userId);
            return await BuildTrendingFallbackAsync(MinRecommendations, ct);
        }

        // Determine category mask from user interests
        var userInterestIds = await context.UserInterests
            .Where(ui => ui.UserId == userId)
            .Select(ui => ui.InterestId)
            .ToListAsync(ct);

        var interestTypeInts = await context.Interests
            .Where(i => userInterestIds.Contains(i.Id))
            .Select(i => i.Type)
            .ToListAsync(ct);

        BookCategory desiredCategories = BookCategory.None;
        foreach (var typeInt in interestTypeInts)
        {
            var it = (InterestType)typeInt;
            if (InterestToCategoryMap.TryGetValue(it, out var cat))
                desiredCategories |= cat;
        }

        // Determine difficulty from academic year
        var difficultyLabel = GetDifficultyLabel(user.AcademicYear);

        // Faculty boost category
        BookCategory facultyBoost = BookCategory.None;
        if (user.Faculty.HasValue && FacultyBoostMap.TryGetValue(user.Faculty.Value, out var fb))
            facultyBoost = fb;

        // Books already rated or read by user
        var excludedBookIds = await context.UserBookInteractions
            .Where(i => i.UserId == userId && (i.Rating.HasValue || i.IsRead))
            .Select(i => i.BookId)
            .ToListAsync(ct);

        // Load all books (EF can't do bitwise flags in SQL easily for flags enum — load + filter in memory)
        var allBooks = await context.Books
            .Where(b => !excludedBookIds.Contains(b.Id))
            .ToListAsync(ct);

        // Score each book
        var scored = allBooks
            .Select(b =>
            {
                double score = 0;

                // Genre match
                if (desiredCategories != BookCategory.None && (b.Category & desiredCategories) != BookCategory.None)
                    score += 3.0;

                // Faculty boost
                if (facultyBoost != BookCategory.None && (b.Category & facultyBoost) != BookCategory.None)
                    score += 1.5;

                return new { Book = b, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var recommendations = scored
            .Take(MinRecommendations)
            .Select(x => new RecommendationDto(
                BookId: x.Book.Id,
                Title: x.Book.Title.Value,
                Author: x.Book.Author.Value,
                Genre: x.Book.Category.ToString(),
                Level: difficultyLabel,
                ReasonLabel: x.Score > 0 ? "Based on your interests" : "Popular",
                Score: x.Score))
            .ToList();

        // Popularity fallback if not enough results
        if (recommendations.Count < MinRecommendations)
        {
            var needed = MinRecommendations - recommendations.Count;
            var alreadyIncluded = recommendations.Select(r => r.BookId).ToHashSet();

            var fallback = await BuildTrendingFallbackAsync(needed * 2, ct);
            var additional = fallback
                .Where(f => !alreadyIncluded.Contains(f.BookId) && !excludedBookIds.Contains(f.BookId))
                .Take(needed)
                .ToList();

            recommendations.AddRange(additional);
        }

        logger.LogDebug("Built {Count} recommendations for user {UserId}.", recommendations.Count, userId);
        return recommendations;
    }

    private async Task<List<RecommendationDto>> BuildTrendingFallbackAsync(int count, CancellationToken ct)
    {
        var trending = await context.Books
            .Select(b => new
            {
                Book = b,
                AvgRating = context.UserBookInteractions
                    .Where(i => i.BookId == b.Id && i.Rating.HasValue)
                    .Select(i => (double?)i.Rating)
                    .Average() ?? 0.0
            })
            .OrderByDescending(x => x.AvgRating)
            .Take(count)
            .ToListAsync(ct);

        return trending.Select(x => new RecommendationDto(
            BookId: x.Book.Id,
            Title: x.Book.Title.Value,
            Author: x.Book.Author.Value,
            Genre: x.Book.Category.ToString(),
            Level: string.Empty,
            ReasonLabel: "Trending",
            Score: x.AvgRating))
            .ToList();
    }

    private static string GetDifficultyLabel(AcademicYear? year)
    {
        return year switch
        {
            AcademicYear.Year1 => "Beginner",
            AcademicYear.Year2 => "Intermediate",
            AcademicYear.Year3 => "Intermediate",
            AcademicYear.Year4 => "Advanced",
            AcademicYear.Postgraduate => "Advanced",
            _ => string.Empty
        };
    }
}
