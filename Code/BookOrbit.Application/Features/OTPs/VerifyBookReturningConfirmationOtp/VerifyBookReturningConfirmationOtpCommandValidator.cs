namespace BookOrbit.Application.Features.OTPs.VerifyBookReturningConfirmationOtp;

public class VerifyBookReturningConfirmationOtpCommandValidator : AbstractValidator<VerifyBookReturningConfirmationOtpCommand>
{
    public VerifyBookReturningConfirmationOtpCommandValidator()
    {
        RuleFor(x => x.BorrowingTransactionId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionIdRules();
    }
}