namespace BookOrbit.Application.Features.Identity.Queries.RefreshTokens;
public class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
{
    public RefreshTokenQueryValidator()
    {
        RuleFor(x => x.RefreshToken)
           .Cascade(CascadeMode.Stop)
           .NotEmpty()
           .WithMessage(RefreshTokenErrors.TokenRequired.Description);
    }
}