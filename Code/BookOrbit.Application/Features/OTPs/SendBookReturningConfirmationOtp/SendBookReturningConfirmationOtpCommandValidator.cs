namespace BookOrbit.Application.Features.OTPs.SendBookReturningConfirmationOtp;

public class SendBookReturningConfirmationOtpCommandValidator : AbstractValidator<SendBookReturningConfirmationOtpCommand>
{
    public SendBookReturningConfirmationOtpCommandValidator()
    {
        RuleFor(x => x.BorrowingTransactionId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionIdRules();
    }
}