namespace BookOrbit.Application.Features.Notifications.Queries.GetNotificationById;

public class GetNotificationByIdQueryValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();

        RuleFor(x => x.NotificationId)
            .Cascade(CascadeMode.Stop)
            .NotificationIdRules();
    }
}
