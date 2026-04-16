namespace BookOrbit.Infrastructure.Services;
public class AppCache : IAppCache
{
    private readonly HybridCacheEntryFlags flags;
    private readonly HybridCache cache;
    private readonly CacheSettings settings ;

    public AppCache(HybridCache cache, IOptions<CacheSettings> settings)
    {
        this.cache = cache;
        this.settings = settings.Value;

        flags = this.settings.UseDistributedCache
            ? HybridCacheEntryFlags.None
            : HybridCacheEntryFlags.DisableDistributedCache;
    }

    public async Task<T> GetOrCreateAsync<T>(
         string key,
         Func<CancellationToken, ValueTask<T>> factory,
         TimeSpan expiration,
         IEnumerable<string> tags,
         CancellationToken ct)
    {
        var options = new HybridCacheEntryOptions()
        {
            Expiration = expiration,
            Flags = flags
        };

        return await cache.GetOrCreateAsync(
            key,
            factory,
            options,
            tags,
            ct);
    }
}