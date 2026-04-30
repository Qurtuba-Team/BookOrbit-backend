
namespace BookOrbit.Infrastructure.Services;
public class RouteService(
    IOptionsSnapshot<Urls> settings) : IRouteService
    
{
    public readonly Urls _settings = settings.Value;

    public string GetBookCoverImageRoute()
    {
        string baseUrl = _settings.BaseUrl;
        string routeRelativeUrl = "uploads/books"; //Could be better , but for now it's fine since we are only using it for book cover images and we know the route is "uploads/books"
        var baseUri = new Uri(baseUrl); 
        var fullUri = new Uri(baseUri, routeRelativeUrl); 
        return fullUri.ToString(); 
    }

    public string GetBookCoverImageRoute(string fileName)
    {
        if (Uri.IsWellFormedUriString(fileName, UriKind.Absolute))
            return fileName;

        string baseUrl = GetBookCoverImageRoute();
        return $"{baseUrl}/{fileName}";
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
