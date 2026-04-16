using BookOrbit.Application.Common.Exceptions;

namespace BookOrbit.Application.Common.Behaviours;
public class CachingBehaviour<TRequest, TResponse>(
    ILogger<CachingBehaviour<TRequest,TResponse>> logger,
    IAppCache appCache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
    where TResponse : IResult

{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        logger.LogInformation("Handling Cache for {RequestName}", typeof(TRequest).Name);

        try
        {
            var result = await appCache.GetOrCreateAsync<TResponse>(
                request.CacheKey,
                async cancelToken =>
                {
                    logger.LogInformation(
                    "Cache factory invoked for {RequestName} with key {CacheKey}",
                    typeof(TRequest).Name,
                    request.CacheKey);

                    var response = await next(cancelToken);

                    if (!response.IsSuccess)
                    {
                        logger.LogWarning(
                        "Handler returned failure. Skipping cache for {RequestName}",
                        typeof(TRequest).Name);

                        throw new CacheFailureException<TResponse>(response);
                    }

                    return response;
                },
                request.Expiration,
                tags: request.Tags,
                cancellationToken: ct);

            return result;
        }
        catch (CacheFailureException<TResponse> ex)
        {
            return ex.Result;
        }
    }
}