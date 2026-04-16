namespace BookOrbit.Domain.Common.ValueObjects;

public record Url(string Value) : ValueObject<string>(Value)
{
    private static string Normalize(string value)
    {
        return
            value
            .Trim();
    }
    private static Result<string> Validate(string value)
    {
        if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
            return UrlErrors.InvalidUrl;

        return value;
    }
    public static Result<Url> Create(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return UrlErrors.RequiredUrl;

        var normalized = Normalize(url);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new Url(validationResult.Value);

        return validationResult.Errors;
    }

}



public static class UrlErrors
{
    private const string className = nameof(Url);


    public static Error InvalidUrl =
        DomainCommonErrors.InvalidProp(className, "Value", "URL", "It must be a valid absolute URL.");

    public static Error RequiredUrl =
        DomainCommonErrors.RequiredProp(className, "Value", "URL");
}

