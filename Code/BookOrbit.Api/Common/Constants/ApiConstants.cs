namespace BookOrbit.Api.Common.Constants;
static public class ApiConstants
{
    public const int NormalRateLimiteMaxRequests = 60;
    public const int NormalRateLimitWindowSpanInMinutes = 1;
    public const int NormalRateLimitSegmentPerWindow = 6;
    public const int NormalRateLimitQueueLimit = 10;


    public const int SensistiveRateLimiteMaxRequests = 5;
    public const int SensistiveRateLimitWindowSpanInMinutes = 1;
    public const int SensistiveRateLimitSegmentPerWindow = 2;
    public const int SensistiveRateLimitQueueLimit = 0;

    public const QueueProcessingOrder RateLimitQueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    public const bool RateLimitAutoReplenishment = true;

    public const string SensitiveRateLimmitingPolicyName = "SensitiveRateLimite";
    public const string NormalRateLimitingPolicyName = "NormalRateLimite";


    public const int ImagesResponseCacheDurationInSeconds = 24 * 60 * 60; // 1 Day
    public const string DefaultOutputCachePolicyName = "DefaultCache";


}

