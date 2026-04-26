using BookOrbit.Infrastructure.Common.Errors;

namespace BookOrbit.Infrastructure.Services;
public class EmailFormatService(ILogger<EmailFormatService> logger) : IEmailFormatService
{
    public const string BookCopyRequestedEmailFormatFilePath = @"Common/EmailTemplates/BookCopyRequestedEmailFormat.html";
    public const string ConfirmEmailFormatFilePath = @"Common/EmailTemplates/ConfirmEmailFormat.html";
    public const string ResetPasswordEmailFormatFilePath = @"Common/EmailTemplates/ResetPasswordEmailFormat.html";
    public const string BorrowingRequestAcceptedEmailFormatFilePath = @"Common/EmailTemplates/BorrowingRequestAcceptedEmailFormat.html";

    private Result<string> GetEmailFormat(string filePath, string placeholder, string value)
    {
        try
        {
            var html = File.ReadAllText(filePath);
            html = html.Replace(placeholder, value);
            return html;
        }
        catch (Exception ex)
        {
            // Handle the exception (e.g., log it)
            logger.LogError(ex, "Error reading email format from file: {FilePath}", filePath);
            return Error.Failure(); // Dont Expose Backend , Just return a general failure error
        }
    }

    public Result<string> BookCopyRequestedEmailFormat(string bookTitle)
    {
        return GetEmailFormat(BookCopyRequestedEmailFormatFilePath, "{{bookTitle}}", bookTitle);
    }

    public Result<string> ConfirmEmailFormat(string link)
    {
        return GetEmailFormat(ConfirmEmailFormatFilePath, "{{link}}", link);
    }

    public Result<string> ResetPasswordEmailFormat(string link)
    {
        return GetEmailFormat(ResetPasswordEmailFormatFilePath, "{{link}}", link);
    }

    public Result<string> BorrowingRequestAcceptedEmailFormat(string bookTitle)
    {
        return GetEmailFormat(BorrowingRequestAcceptedEmailFormatFilePath, "{{bookTitle}}", bookTitle);
    }
}