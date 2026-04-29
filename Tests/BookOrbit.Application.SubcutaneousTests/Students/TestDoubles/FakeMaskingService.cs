namespace BookOrbit.Application.SubcutaneousTests.Students.TestDoubles;

using BookOrbit.Application.Common.Interfaces;

internal sealed class FakeMaskingService : IMaskingService
{
    public string MaskEmail(string email) => email;

    public string MaskPhoneNumber(string phoneNumber) => phoneNumber;

    public string MaskTelegramUserId(string telegramUserId) => telegramUserId;
}
