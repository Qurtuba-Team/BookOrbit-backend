namespace BookOrbit.Domain.Books.ValueObjects;

public record ISBN(string Value) : ValueObject<string>(Value)
{
    private static readonly Regex IsbnRegex =
        new(@"^(?:[0-9]{9}[0-9X]|97[89][0-9]{10})$", RegexOptions.Compiled);

    public const int MinLength = 10;
    public const int MaxLength = 13;

    public static string Normalize(string value)
    {
        return value
            .Trim()
            .Replace("-", "")
            .Replace(" ", "");
    }
    private static Result<string> Validate(string value)
    {
        if (value.Length != MaxLength && value.Length != MinLength)
            return BookErrors.InvalidISBN;


        if (!IsbnRegex.IsMatch(value))
            return BookErrors.InvalidISBN;

        return value;
    }
    public static Result<ISBN> Create(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return BookErrors.ISBNRequired;

        var normalized = Normalize(isbn);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new ISBN(validationResult.Value);

        return validationResult.Errors;
    }
}