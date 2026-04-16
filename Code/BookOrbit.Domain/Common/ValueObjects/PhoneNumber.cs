namespace BookOrbit.Domain.Common.ValueObjects;

public record PhoneNumber(string Value) : ValueObject<string>(Value)
{
    private static readonly Regex PhoneNumberRegex =
       new(@"^(?:01|201)[0125][0-9]+$", RegexOptions.Compiled);

    public const int MinLength = 11;
    public const int MaxLength = 12;
    static public readonly List<string> Prefixes = ["01", "201"];

    public static string Normalize(string value)
    {
        var normalized = value
          .Trim()
         .Replace(" ", "")
         .Replace("-", "")
         .Replace("+", "");

        return normalized.StartsWith("01") ?
            ("2" + normalized) :
            normalized;
    }
    private static Result<string> Validate(string value)
    {
        if(value.Length < MinLength || value.Length > MaxLength)
            return PhoneNumberErrors.InvalidPhoneNumber;        

        if (!PhoneNumberRegex.IsMatch(value))
            return PhoneNumberErrors.InvalidPhoneNumber;

        return value;
    }

    public static Result<PhoneNumber> Create(string? phoneNumber)
    {
        // Hot path, so we check for null or whitespace before normalization and validation to avoid unnecessary processing.
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return PhoneNumberErrors.RequiredPhoneNumber;


        var normalized = Normalize(phoneNumber);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new PhoneNumber(validationResult.Value);

        return validationResult.Errors;
    }
}

static public class PhoneNumberErrors
{
    private const string className = nameof(PhoneNumber);

    public static readonly Error InvalidPhoneNumber = DomainCommonErrors.InvalidProp(
                className,
                "Value",
                "Phone number",
                "It must be a valid Egyptian phone number."
            );

    public static readonly Error RequiredPhoneNumber = DomainCommonErrors.RequiredProp(
                className,
                "Value",
                "Phone number"
            );
}