namespace BookOrbit.Application.Features.OTPs.VerifyBookReturningConfirmationOtp;

public class VerifyBookReturningConfirmationOtpCommandHandler(
    ILogger<VerifyBookReturningConfirmationOtpCommandHandler> logger,
    IBorrowingTransactionOtpService otpService,
    TimeProvider timeProvider) : IRequestHandler<VerifyBookReturningConfirmationOtpCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(VerifyBookReturningConfirmationOtpCommand command, CancellationToken ct)
    {
        var isValidOtpResult = await otpService.VerifyOtp(
            command.Otp,
            command.BorrowingTransactionId,
            timeProvider.GetUtcNow(),
            ct);

        if (isValidOtpResult.IsFailure)
        {
            logger.LogWarning("Failed to verify OTP for borrowing transaction with id {BorrowingTransactionId}. Errors: {Errors}",
                command.BorrowingTransactionId, isValidOtpResult.Errors);
            return isValidOtpResult.Errors;
        }
        
        logger.LogInformation("Successfully verified OTP for borrowing transaction with id {BorrowingTransactionId}.", command.BorrowingTransactionId);
        return Result.Success;
    }
}