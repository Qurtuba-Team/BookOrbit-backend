namespace BookOrbit.Application.Features.Identity.Commands.SendResetPassword;
public class SendResetPasswordCommandValidator : AbstractValidator<SendResetPasswordCommand>
{
    public SendResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(IdentityApplicationErrors.EmailRequired.Description)
            .EmailAddress().WithMessage(IdentityApplicationErrors.InvalidEmail.Description);
    }
}