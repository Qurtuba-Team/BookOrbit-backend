namespace BookOrbit.Application.Common.Interfaces;
public interface IAppCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan expiration,
        IEnumerable<string> tags,
        CancellationToken cancellationToken);
}