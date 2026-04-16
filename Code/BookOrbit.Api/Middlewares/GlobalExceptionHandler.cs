namespace BookOrbit.Api.Middlewares;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = GetSafeMessage(exception),
            Type = "https://httpstatuses.com/500"
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static string GetSafeMessage(Exception exception)
    {
        return exception switch
        {
            // Authentication / Authorization
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            SecurityTokenException => "Invalid or expired authentication token.",

            //  Validation / Input
            ArgumentNullException => "Required data is missing.",
            ArgumentException => "Invalid request data.",
            FormatException => "Invalid data format.",
            InvalidOperationException => "The requested operation is not valid in the current state.",

            //  Serialization / JSON
            NotSupportedException => "The requested operation is not supported.",
            JsonException => "Invalid request payload format.",

            //  HTTP / External calls
            HttpRequestException => "Error occurred while communicating with an external service.",
            TaskCanceledException => "The request timed out.",

            //  Database
            DbUpdateException => "A database error occurred while processing your request.",

            //  Not Found
            KeyNotFoundException => "The requested resource was not found.",


            //  Fallback
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}