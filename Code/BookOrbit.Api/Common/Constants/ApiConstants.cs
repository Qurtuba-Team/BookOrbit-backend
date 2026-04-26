namespace BookOrbit.Api.Common.Constants;
static public class ApiConstants
{
    public const int NormalRateLimiteMaxRequests = 60;
    public const int NormalRateLimitWindowSpanInMinutes = 1;
    public const int NormalRateLimitSegmentPerWindow = 6;
    public const int NormalRateLimitQueueLimit = 10;


    public const int SensitiveRateLimitMaxRequests = 5;
    public const int SensitiveRateLimitWindowSpanInMinutes = 1;
    public const int SensitiveRateLimitSegmentPerWindow = 2;
    public const int SensitiveRateLimitQueueLimit = 0;

    public const string OnceAMinuteRateLimitingPolicyName = "OnceAMinuteRateLimit";
    public const int OnceAMinuteRateLimitMaxRequests = 1;
    public const int OnceAMinuteRateLimitWindowSpanInMinutes = 1;
    public const int OnceAMinuteRateLimitSegmentPerWindow = 1;
    public const int OnceAMinuteRateLimitQueueLimit = 0;

    public const QueueProcessingOrder RateLimitQueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    public const bool RateLimitAutoReplenishment = true;

    public const string SensitiveRateLimitingPolicyName = "SensitiveRateLimite";
    public const string NormalRateLimitingPolicyName = "NormalRateLimite";


    public const int ImagesResponseCacheDurationInSeconds = 24 * 60 * 60; // 1 Day
    public const string DefaultOutputCachePolicyName = "DefaultCache";


}

