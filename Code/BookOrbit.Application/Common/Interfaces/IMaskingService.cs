namespace BookOrbit.Application.Common.Interfaces;
public interface IMaskingService
{
    public string MaskEmail(string email);
    public string MaskPhoneNumber(string phoneNumber);
    public string MaskTelegramUserId(string telegramUserId);
}

