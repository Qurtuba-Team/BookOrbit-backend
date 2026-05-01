
namespace BookOrbit.Infrastructure.Services;
public class RouteService(
    IOptionsSnapshot<Urls> settings) : IRouteService
    
{
    private readonly Urls _settings = settings.Value;

    public string GetBookCoverImageRoute()
    {
        // Return the versioned API endpoint base path.
        // All book-cover image requests must flow through ImagesController
        // so that the [Authorize] pipeline is always enforced.
        string baseUrl = _settings.BaseUrl.TrimEnd('/');
        return $"{baseUrl}/api/v1/images/books";
    }

    public string GetBookCoverImageRoute(string fileName)
    {
        // External cover URLs (auto-retrieved from Open Library / Google Books)
        // are stored as-is — pass them straight back so the caller can use them
        // directly without wrapping in an API route.
        if (Uri.IsWellFormedUriString(fileName, UriKind.Absolute))
            return fileName;

        // Local uploads and the default cover image are served through the
        // authenticated ImagesController route.
        return $"{GetBookCoverImageRoute()}/{fileName}";
    }

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
