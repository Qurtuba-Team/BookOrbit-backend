namespace BookOrbit.Application.Features.Identity.Commands.SendEmailConfirmation;
public class SendEmailConfirmationCommandValidator : AbstractValidator<SendEmailConfirmationCommand>
{
    public SendEmailConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(IdentityApplicationErrors.EmailRequired.Description)
            .EmailAddress().WithMessage(IdentityApplicationErrors.InvalidEmail.Description);
    }
}