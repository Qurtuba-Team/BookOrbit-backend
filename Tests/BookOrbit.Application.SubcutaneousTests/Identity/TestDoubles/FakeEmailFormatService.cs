namespace BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Domain.Common.Results;

internal sealed class FakeEmailFormatService : IEmailFormatService
{
    public Result<string> ConfirmEmailResult { get; set; } = "confirm-body";
    public Result<string> ResetPasswordResult { get; set; } = "reset-body";

    public Result<string> ConfirmEmailFormat(string link)
        => ConfirmEmailResult;

    public Result<string> ResetPasswordEmailFormat(string link)
        => ResetPasswordResult;

    public Result<string> BookCopyRequestedEmailFormat(string bookTitle)
        => "book-requested";

    public Result<string> BorrowingRequestAcceptedEmailFormat(string bookTitle)
        => "borrowing-accepted";

    public Result<string> BookDeliveryConfirmationEmailFormat(string otp)
        => $"book-delivery-confirmation-{otp}";

    public Result<string> BookReturningConfirmationEmailFormat(string otp)
        => $"book-returning-confirmation-{otp}";
}
