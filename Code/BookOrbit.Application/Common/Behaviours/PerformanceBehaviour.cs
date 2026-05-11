
using BookOrbit.Application.Features.Identity.Queries.GenerateTokens;
using BookOrbit.Application.Features.Students.Commands.CreateStudent;

namespace BookOrbit.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<TRequest> logger,
    ICurrentUser user,
    IIdentityService identityService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch stopWatch = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        stopWatch.Start();

        var response = await next(ct);

        stopWatch.Stop();

        var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;

        if(elapsedMilliseconds>500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = user.Id ?? string.Empty;
            var userName = string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                userName = await identityService.GetUserNameAsync(userId,ct);
            }

            if(request is 
                GenerateTokenQuery
                or CreateStudentCommand)
            {
                //very secret information , cannot log the full request
                logger.LogWarning(
                    "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}", requestName, elapsedMilliseconds, userId, userName, "Sensitive Information");
            }
            else
            {
                logger.LogWarning(
                    "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}", requestName, elapsedMilliseconds, userId, userName, request);
            }
        }

        return response;
    }
}
