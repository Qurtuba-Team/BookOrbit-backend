namespace BookOrbit.Infrastructure.Settings;
public class AppSettings
{
    public string CorsPolicyName { get; set; } = string.Empty;
    public string[] AllowedOrigins { get; set; } = [];
}