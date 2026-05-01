namespace BookOrbit.Application.Features.OTPs.SendBookDeliveryConfirmationOtp;
public class SendBookDeliveryConfirmationOtpCommandValidator : AbstractValidator<SendBookDeliveryConfirmationOtpCommand>
{
    public SendBookDeliveryConfirmationOtpCommandValidator()
    {
        RuleFor(x => x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();
    }
}