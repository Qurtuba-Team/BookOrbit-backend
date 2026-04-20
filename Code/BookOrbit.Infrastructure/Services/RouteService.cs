using Microsoft.AspNetCore.WebUtilities;

namespace BookOrbit.Infrastructure.Services;
public class RouteService(
    IOptionsSnapshot<Urls> settings) : IRouteService
{
    public readonly Urls _settings = settings.Value;

    public string GetEmailConfirmationRoute(string email, string token)
    {
        string baseUrl = _settings.FrontEndUrl;
        string endPointRelativeUrl = _settings.ConfirmEmailFrontEndRelativeUrl;

        var baseUri = new Uri(baseUrl);
        var fullUri = new Uri(baseUri, endPointRelativeUrl);

        var queryParams = new Dictionary<string, string>
          {
        { "token", token },
        { "email", email }
         };

        string finalUrl = QueryHelpers.AddQueryString(fullUri.ToString(), queryParams!);

        return finalUrl;
    }

    public string GetResetPasswordRoute(string email, string token)
    {
        string baseUrl = _settings.FrontEndUrl;
        string endPointRelativeUrl = _settings.ResetPasswordFrontEndRelativeUrl;
        var baseUri = new Uri(baseUrl);
        var fullUri = new Uri(baseUri, endPointRelativeUrl);
        var queryParams = new Dictionary<string, string>
        {
        { "token", token },
        { "email", email }
         };
        string finalUrl = QueryHelpers.AddQueryString(fullUri.ToString(), queryParams!);
        return finalUrl;
    }
}