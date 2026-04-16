namespace BookOrbit.Application.Features.Identity.Queries.GenerateTokens;
public class GenerateTokenQueryValidator : AbstractValidator<GenerateTokenQuery>
{
    public GenerateTokenQueryValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .EmailRules();

        RuleFor(x=> x.Password)
            .Cascade(CascadeMode.Stop)
            .PasswordRules();
    }
}
