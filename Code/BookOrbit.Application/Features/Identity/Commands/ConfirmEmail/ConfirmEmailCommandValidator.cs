namespace BookOrbit.Application.Features.Identity.Commands.ConfirmEmail;
public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(IdentityApplicationErrors.IdRequired.Description);

        RuleFor(x => x.EncodedToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(IdentityApplicationErrors.EmailConfirmationTokenRequired.Description);
    }
}
