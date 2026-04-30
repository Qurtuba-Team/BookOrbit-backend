namespace BookOrbit.Application.Common.OTPs.SendOtp;
public class SendBookDeliveryConfirmationOtpCommandValidator : AbstractValidator<SendBookDeliveryConfirmationOtpCommand>
{
    public SendBookDeliveryConfirmationOtpCommandValidator()
    {
        RuleFor(x => x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();
    }
}