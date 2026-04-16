namespace BookOrbit.Domain.Books.ValueObjects;
public record BookAuthor(string Value) : ValueObject<string>(Value)
{

    private static readonly Regex BookAuthorRegex =
       new(@"^[A-Za-z\u0600-\u06FF ]+$", RegexOptions.Compiled);

    public const int MinLength = 3;
    public const int MaxLength = 150;


    public static string Normalize(string value)
    {
        return value
          .Trim();
    }
    private static Result<string> Validate(string value)
    {
        if (value.Length > MaxLength || value.Length < MinLength)
            return BookErrors.InvalidAuthor;


        if (!BookAuthorRegex.IsMatch(value))
            return BookErrors.InvalidAuthor;

        return value;
    }
    public static Result<BookAuthor> Create(string? bookAuthor)
    {
        // Hot path, so we check for null or whitespace before normalization and validation to avoid unnecessary processing.
        if (string.IsNullOrWhiteSpace(bookAuthor))
            return BookErrors.AuthorRequired;


        var normalized = Normalize(bookAuthor);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new BookAuthor(validationResult.Value);

        return validationResult.Errors;
    }

}