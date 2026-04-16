namespace BookOrbit.Infrastructure.Settings;
public class CacheSettings
{
    public bool UseDistributedCache { get; set; }
    public int RemoteCachExpirationInMinutes { get; set; } 
    public int LocalCachExpirationInSeconds { get; set; } 
    public int OutputCachExpirationInSeconds { get; set; } 
}