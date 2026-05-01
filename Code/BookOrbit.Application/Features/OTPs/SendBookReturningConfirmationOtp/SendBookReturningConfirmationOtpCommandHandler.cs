namespace BookOrbit.Application.Features.OTPs.SendBookReturningConfirmationOtp;

public class SendBookReturningConfirmationOtpCommandHandler(
    ILogger<SendBookReturningConfirmationOtpCommandHandler> logger,
    IBorrowingTransactionOtpService otpService,
    IEmailFormatService emailFormatService,
    IEmailService emailService,
    IAppDbContext context,
    TimeProvider timeProvider) : IRequestHandler<SendBookReturningConfirmationOtpCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(SendBookReturningConfirmationOtpCommand command, CancellationToken ct)
    {
        var studentEmailResult = await context.BorrowingTransactions
            .AsNoTracking()
            .Select(bt => new
            {
                bt.Id,
                Email = bt.LenderStudent!.UniversityMail.Value
            })
            .FirstOrDefaultAsync(bt => bt.Id == command.BorrowingTransactionId, cancellationToken: ct);

        if (studentEmailResult is null)
        {
            logger.LogWarning("Student For Borrowing Transaction {BorrowingTransactionId} Not Found", command.BorrowingTransactionId);
            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        var otpGeneratingResult = await otpService.GenerateOtp(command.BorrowingTransactionId, timeProvider.GetUtcNow(), ct);

        if (otpGeneratingResult.IsFailure)
        {
            logger.LogError("Failed to generate OTP for borrowing transaction {BorrowingTransactionId} to email {Email}. Errors: {Errors}",
                command.BorrowingTransactionId, studentEmailResult.Email, string.Join(", ", otpGeneratingResult.Errors.Select(e => e.Description)));
            return otpGeneratingResult.Errors;
        }

        var emailFormatResult = emailFormatService.BookReturningConfirmationEmailFormat(otpGeneratingResult.Value);

        if (emailFormatResult.IsFailure)
        {
            logger.LogWarning("Email format generation failed for email {Email}: {Errors}",
                studentEmailResult.Email,
                emailFormatResult.Errors);
            return emailFormatResult.Errors;
        }

        var emailResult = await emailService.SendEmailAsync(
            studentEmailResult.Email,
            "Book Returning confirmation OTP",
            emailFormatResult.Value);

        if (emailResult.IsFailure)
            return emailResult.Errors;

        return Result.Success;
    }
}