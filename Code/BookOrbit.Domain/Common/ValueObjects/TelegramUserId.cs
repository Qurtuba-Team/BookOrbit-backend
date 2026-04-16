namespace BookOrbit.Domain.Common.ValueObjects;

public record TelegramUserId(string Value) : ValueObject<string>(Value)
{
    private static readonly Regex TelegramUserIdRegex =
       new(@"^(?=(?:[0-9_]*[a-z]){3})[a-z0-9_]+$", RegexOptions.Compiled);

    public const int MinLength = 5;
    public const int MaxLength = 32;

    public static string Normalize(string value)
    {
        return 
            value
            .Trim()
            .ToLower();
    }
    private static Result<string> Validate(string value)
    {
        if(value.Length > MaxLength || value.Length < MinLength)
            return TelegramUserIdErrors.InvalidTelegramUserId;

        if (!TelegramUserIdRegex.IsMatch(value))
            return TelegramUserIdErrors.InvalidTelegramUserId;

        return value;
    }
    public static Result<TelegramUserId> Create(string? telegramUserId)
    {
        if (string.IsNullOrWhiteSpace(telegramUserId))
            return TelegramUserIdErrors.TelegramUserIdRequired;

        var normalized = Normalize(telegramUserId);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new TelegramUserId(validationResult.Value);

        return validationResult.Errors;
    }

}

public static class TelegramUserIdErrors
{
    private const string className = nameof(TelegramUserId);

    public static readonly Error InvalidTelegramUserId = DomainCommonErrors.InvalidProp(
                className,
                "Value",
                "Telegram User ID",
                "It must be a valid telegram user Id.");

    public static readonly Error TelegramUserIdRequired = DomainCommonErrors.RequiredProp(
                className,
                "Value",
                "Telegram User ID");

}

