namespace BookOrbit.Application.Common.OTPs.VerifyBookDeliveryConfirmationOtp;
public class VerifyBookDeliveryConfirmationOtpCommandHandler(
    IAppDbContext context,
    ILogger<VerifyBookDeliveryConfirmationOtpCommandHandler> logger,
    IOtpService otpService) : IRequestHandler<VerifyBookDeliveryConfirmationOtpCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(VerifyBookDeliveryConfirmationOtpCommand command, CancellationToken ct)
    {
        var studentExists = await context.BorrowingRequests
            .AsNoTracking()
            .AnyAsync(br => br.Id == command.BorrowingRequestId, ct);

        if (!studentExists)
        {
            logger.LogWarning("Student For Borrwoing Request {BorroiwngRequestId}", command.BorrowingRequestId);
            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        var isValidOtp = await otpService.VerifyBorrwoingReuqestOtp(
            command.Otp,
            command.BorrowingRequestId,
            ct);


        return Result.Success;
    }
}