namespace BookOrbit.Domain.Students.ValueObjects;

public record StudentName(string Value) : ValueObject<string>(Value)
{
    //Arabic , english , spaces
    private static readonly Regex NameRegex =
   new(@"^[A-Za-z ]+$", RegexOptions.Compiled);

    public const int MinLength = 3;
    public const int MaxLength = 50;


    public static string Normalize(string value)
    {
        return value
            .Trim();
    }

    private static Result<string> Validate(string value)
    {
        if (value.Length > MaxLength || value.Length < MinLength)
            return StudentErrors.InvalidName;

        if (!NameRegex.IsMatch(value))
            return StudentErrors.InvalidName;

        return value;
    }
    public static Result<StudentName> Create(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return StudentErrors.NameRequired;

        var normalized = Normalize(name);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new StudentName(validationResult.Value);

        return validationResult.Errors;

    }

}

