
namespace BookOrbit.Application.Common.Constants;
public static class BookCoverCachingConstants
{
    public const string BookCoverTag = "book-cover";

    /// <summary>
    /// Prefix used for all book-cover cache keys. Combined with the SHA-256 hash
    /// of isbn + title to produce a bounded, collision-resistant cache key.
    /// </summary>
    public const string CacheKeyPrefix = "book-cover:";

    /// <summary>
    /// How long a resolved cover URL is kept in the cache before it is re-fetched.
    /// 24 h is long enough to avoid hammering the external APIs while staying fresh.
    /// </summary>
    public static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);
}
