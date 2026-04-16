namespace BookOrbit.Domain.Students.ValueObjects;

public record UniversityMail(string Value) : ValueObject<string>(Value)
{
    private static readonly Regex UniversityMailRegex =
   new(@"^[A-Za-z0-9._%+-]+@std\.mans\.edu\.eg$", RegexOptions.Compiled);

    public const int MaxLength = 320;


    public static string Normalize(string value)
    {
        return value
            .Trim()
            .ToLower();
    }
    private static Result<string> Validate(string value)
    {
        if(value.Length > MaxLength)
            return StudentErrors.InvalidUniversityMail;

        if (!UniversityMailRegex.IsMatch(value))
            return StudentErrors.InvalidUniversityMail;

        return value;
    }
    public static Result<UniversityMail> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return StudentErrors.UniversityMailRequired;

        var normalized = Normalize(email);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new UniversityMail(validationResult.Value);

        return validationResult.Errors;
    }

}

