namespace BookOrbit.Infrastructure.Services;

internal class MaskingService : IMaskingService
{
    public string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return email; //unvalid

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"{localPart[0]}***@{domain}";

        var visibleStart = localPart[0];
        var visibleEnd = localPart[^1];
        var maskedMiddle = new string('*', localPart.Length - 2);

        return $"{visibleStart}{maskedMiddle}{visibleEnd}@{domain}";
    }

    public string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;


        if (phoneNumber.Length <= 4)
            return new string('*', phoneNumber.Length);

        var visibleStart = phoneNumber.Substring(0, 2);
        var visibleEnd = phoneNumber.Substring(phoneNumber.Length - 2);
        var maskedMiddle = new string('*', phoneNumber.Length - 4);

        return $"{visibleStart}{maskedMiddle}{visibleEnd}";
    }

    public string MaskTelegramUserId(string telegramUserId)
    {
        if (string.IsNullOrWhiteSpace(telegramUserId))
            return string.Empty;

        if (telegramUserId.Length <= 4)
            return new string('*', telegramUserId.Length);

        var visibleStart = telegramUserId.Substring(0, 2);
        var visibleEnd = telegramUserId.Substring(telegramUserId.Length - 2);
        var maskedMiddle = new string('*', telegramUserId.Length - 4);

        return $"{visibleStart}{maskedMiddle}{visibleEnd}";
    }
}