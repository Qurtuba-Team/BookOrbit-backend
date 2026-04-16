using System.Diagnostics;

namespace BookOrbit.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> logger;
    private readonly Stopwatch stopWatch;
    private readonly ICurrentUser user;
    private readonly IIdentityService identityService;
    public PerformanceBehaviour(
        ILogger<TRequest> logger,
        ICurrentUser user,
        IIdentityService identityService)
    {
        this.logger = logger;
        stopWatch = new();
        this.user = user;
        this.identityService = identityService;
    }



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

            logger.LogWarning(
            "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}", requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}
