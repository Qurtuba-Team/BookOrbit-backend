namespace BookOrbit.Application.Features.Notifications;

public static class NotificationGeneralValidation
{
    public static IRuleBuilder<T, Guid> NotificationIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(NotificationErrors.IdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(NotificationErrors.IdRequired.Description);
}
