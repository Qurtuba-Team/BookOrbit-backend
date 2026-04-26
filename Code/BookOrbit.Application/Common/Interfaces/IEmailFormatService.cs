namespace BookOrbit.Application.Common.Interfaces;
public interface IEmailFormatService
{
    Result<string> ConfirmEmailFormat(string link);
    Result<string> ResetPasswordEmailFormat(string link);
    Result<string> BookCopyRequestedEmailFormat(string bookTitle);
    Result<string> BorrowingRequestAcceptedEmailFormat(string bookTitle);
}