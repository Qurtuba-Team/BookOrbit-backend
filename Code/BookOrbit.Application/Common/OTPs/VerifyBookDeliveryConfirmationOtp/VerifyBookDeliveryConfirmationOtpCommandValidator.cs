namespace BookOrbit.Application.Common.OTPs.VerifyBookDeliveryConfirmationOtp;
public class VerifyBookDeliveryConfirmationOtpCommandValidator : AbstractValidator<VerifyBookDeliveryConfirmationOtpCommand>
{
    public VerifyBookDeliveryConfirmationOtpCommandValidator()
    {
        RuleFor(x=>x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();
    }
}