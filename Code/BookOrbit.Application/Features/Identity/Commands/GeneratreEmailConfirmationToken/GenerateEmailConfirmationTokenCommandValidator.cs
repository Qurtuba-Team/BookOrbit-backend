namespace BookOrbit.Application.Features.Identity.Commands.GeneratreEmailConfirmationToken;
public class GenerateEmailConfirmationTokenCommandValidator : AbstractValidator<GenerateEmailConfirmationTokenCommand>
{
    public GenerateEmailConfirmationTokenCommandValidator()
    {
        RuleFor(x => x.UserId)
             .NotEmpty().WithMessage(IdentityApplicationErrors.IdRequired.Description);
    }
}