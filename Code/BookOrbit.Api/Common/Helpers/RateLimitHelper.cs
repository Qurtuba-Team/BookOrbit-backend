namespace BookOrbit.Api.Common.Helpers;
public static class RateLimitHelper
{
    public static void AddSlidingPolicy(
        RateLimiterOptions options,
        string policyName,
        int permitLimit,
        int windowMinutes,
        int segmentsPerWindow,
        int queueLimit)
    {
        options.AddPolicy(policyName, context =>
        {
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var key = userId
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "anon";

            return RateLimitPartition.GetSlidingWindowLimiter(key, _ =>
                new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromMinutes(windowMinutes),
                    SegmentsPerWindow = segmentsPerWindow,
                    QueueLimit = queueLimit,
                    QueueProcessingOrder = ApiConstants.RateLimitQueueProcessingOrder,
                    AutoReplenishment = ApiConstants.RateLimitAutoReplenishment
                });
        });
    }
}
