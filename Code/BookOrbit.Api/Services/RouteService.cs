using BookOrbit.Application.Common.Errors;
using Microsoft.Extensions.Options;

namespace BookOrbit.Api.Services;
public class RouteService(
    LinkGenerator linkGenerator,
    IOptions<AppSettings> appSettings) : IRouteService
{
    private readonly AppSettings _settings = appSettings.Value;

    public Result<string> GetRouteByName(string? routeName, object? values)
    {
        if (string.IsNullOrWhiteSpace(routeName))
        {
            return ApplicationCommonErrors.NotFoundClass("RouteName", "RouteName", "Route Name");
        }

        if (!Uri.TryCreate(_settings.BaseUrl, UriKind.Absolute, out var uri))
        {
            return ApplicationCommonErrors.NotFoundClass("BaseUrl", "BaseUrl", "Base URL");
        }

        var link = linkGenerator.GetUriByName(
            routeName,
            values,
            scheme: uri.Scheme,
            host: new HostString(uri.Host, uri.Port));

        if (link is null)
        {
            return ApplicationCommonErrors.NotFoundClass("Route", "RouteName", "Route Name");
        }

        return link;
    }
}
