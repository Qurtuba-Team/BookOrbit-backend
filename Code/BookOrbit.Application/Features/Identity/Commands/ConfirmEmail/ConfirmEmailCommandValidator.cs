namespace BookOrbit.Application.Features.Identity.Commands.ConfirmEmail;
public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(IdentityApplicationErrors.EmailRequired.Description)
            .EmailAddress().WithMessage(IdentityApplicationErrors.InvalidEmail.Description);

        RuleFor(x => x.EncodedToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(IdentityApplicationErrors.EmailConfirmationTokenRequired.Description);
    }
}
