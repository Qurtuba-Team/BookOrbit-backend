namespace BookOrbit.Application.Features.Identity.Commands.ResetPassowrd;
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
    .Cascade(CascadeMode.Stop)
    .NotEmpty().WithMessage(IdentityApplicationErrors.EmailRequired.Description)
    .EmailAddress().WithMessage(IdentityApplicationErrors.InvalidEmail.Description);

        RuleFor(x => x.EncodedToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(IdentityApplicationErrors.PasswordResetTokenRequired.Description);
    }
}