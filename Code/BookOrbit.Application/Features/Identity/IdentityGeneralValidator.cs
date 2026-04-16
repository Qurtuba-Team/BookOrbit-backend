using BookOrbit.Domain.Identity;

namespace BookOrbit.Application.Features.Identity;
static public class IdentityGeneralValidator
{

    static public IRuleBuilder<T, string> EmailRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
        .NotEmpty().WithMessage(IdentityApplicationErrors.EmailRequired.Description)
        .EmailAddress().WithMessage(IdentityApplicationErrors.InvalidEmail.Description);

    static public IRuleBuilder<T, string> PasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
        .NotEmpty().WithMessage(IdentityApplicationErrors.PasswordRequired.Description)
        .Matches("^(?=.*[a-zA-Z])(?=.*\\d).{6,}$").WithMessage(IdentityApplicationErrors.InvalidPassword.Description);
}