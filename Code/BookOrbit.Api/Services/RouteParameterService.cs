namespace BookOrbit.Api.Services;
public class RouteParameterService(IHttpContextAccessor httpContextAccessor) : IRouteParameterService
{
    public Result<string> GetRouteParameter(string parameterName)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return Error.Failure("HttpContext is null");
        }

        if (!httpContext.Request.RouteValues.TryGetValue(parameterName, out var value))
        {
            return Error.Validation($"Route parameter '{parameterName}' not found");
        }

        if (value is not string stringValue || string.IsNullOrWhiteSpace(stringValue))
        {
            return Error.Validation($"Route parameter '{parameterName}' is invalid");
        }

        return stringValue;
    }
}