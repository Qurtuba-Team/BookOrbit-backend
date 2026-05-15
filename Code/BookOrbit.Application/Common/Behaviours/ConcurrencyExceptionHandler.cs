
namespace BookOrbit.Application.Common.Behaviours;

public class ConcurrencyExceptionHandler<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest
    where TResponse : IResult,IErrorFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return TResponse.FromErrors([Error.Conflict("Concurrency", "A concurrency conflict occurred.")]);
        }
    }
}
