namespace BookOrbit.Api.Common.Mappers
{
    public static class ErrorToProblemMapper
    {
        public static ProblemDetails Map(List<Error>? errors, HttpContext? context = null)
        {
            if (errors == null || errors.Count == 0)
            {
                return new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Unknown error",
                    Type = "https://httpstatuses.com/500",
                    Instance = context?.Request.Path
                };
            }

            if (errors.All(e => e.Type == ErrorKind.Validation))
            {
                return new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "One or more validation errors occurred.",
                    Type = "https://httpstatuses.com/400",
                    Extensions =
                        {
                            ["errors"] = errors.Select(e => e.Description)
                        }
                };
            }
            var error = errors[0];

            var statusCode = error.Type switch
            {
                ErrorKind.Conflict => StatusCodes.Status409Conflict,
                ErrorKind.Validation => StatusCodes.Status400BadRequest,
                ErrorKind.NotFound => StatusCodes.Status404NotFound,
                ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError,
            };

            return new ProblemDetails
            {
                Status = statusCode,
                Title = error.Type.ToString(),
                Detail = error.Description,
                Type = $"https://httpstatuses.com/{statusCode}",
                Instance = context?.Request.Path,
                Extensions =
        {
            ["errors"] = errors.Select(e => e.Description)
        }
            };
        }
    }
}
