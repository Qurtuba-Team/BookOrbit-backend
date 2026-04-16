namespace BookOrbit.Domain.Books.ValueObjects;
public record BookPublisher(string Value) : ValueObject<string>(Value)
{

    private static readonly Regex BookPublisherRegex =
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
            return BookErrors.InvalidPublisher;


        if (!BookPublisherRegex.IsMatch(value))
            return BookErrors.InvalidPublisher;

        return value;
    }
    public static Result<BookPublisher> Create(string? bookPublisher)
    {
        // Hot path, so we check for null or whitespace before normalization and validation to avoid unnecessary processing.
        if (string.IsNullOrWhiteSpace(bookPublisher))
            return BookErrors.PublisherRequired;


        var normalized = Normalize(bookPublisher);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new BookPublisher(validationResult.Value);

        return validationResult.Errors;
    }

}