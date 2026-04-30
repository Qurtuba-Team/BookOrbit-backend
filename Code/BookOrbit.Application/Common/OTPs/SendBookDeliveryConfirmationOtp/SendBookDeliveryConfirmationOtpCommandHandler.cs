namespace BookOrbit.Application.Common.OTPs.SendOtp;
public class SendBookDeliveryConfirmationOtpCommandHandler (
    ILogger<SendBookDeliveryConfirmationOtpCommandHandler> logger,
    IOtpService otpService,
    IEmailFormatService emailFormatService,
    IEmailService emailService,
    IAppDbContext context): IRequestHandler<SendBookDeliveryConfirmationOtpCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(SendBookDeliveryConfirmationOtpCommand command, CancellationToken ct)
    {
        var studentEmailResult = await context.BorrowingRequests
            .AsNoTracking()
            .Select(br => new
            {
                br.Id,
                Email = br.BorrowingStudent!.UniversityMail.Value
            })
            .FirstOrDefaultAsync(br=>br.Id == command.BorrowingRequestId);

        if(studentEmailResult is null)
        {
            logger.LogWarning("Student For Borrwoing Request {BorroiwngRequestId}",command.BorrowingRequestId);
            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var otpGeneratingResult = await otpService.GenerateBorrowingRequestOtp(command.BorrowingRequestId, ct);

        if (otpGeneratingResult.IsFailure)
        {
            logger.LogError("Failed to generate OTP for borrowing request {BorrowingRequestId} to email {Email}. Errors: {Errors}",
                command.BorrowingRequestId, studentEmailResult.Email, string.Join(", ", otpGeneratingResult.Errors.Select(e => e.Description)));
            return otpGeneratingResult.Errors;
        }

        var emailFormatResult = emailFormatService.BookDeliveryConfirmationEmailFormat(otpGeneratingResult.Value);

        if (emailFormatResult.IsFailure)
        {
            logger.LogWarning("Email format generation failed for email {Email}: {Errors}",
                studentEmailResult.Email,
                emailFormatResult.Errors);
            return emailFormatResult.Errors;
        }

        var emailResult = await emailService.SendEmailAsync(
        studentEmailResult.Email,
        "Book Delivery confirmation OTP",
        emailFormatResult.Value);

        if (emailResult.IsFailure)
            return emailResult.Errors;

        return Result.Success;
    }
}