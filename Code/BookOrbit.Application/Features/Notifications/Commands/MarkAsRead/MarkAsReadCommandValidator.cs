namespace BookOrbit.Application.Features.Notifications.Commands.MarkAsRead;
public class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x=>x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}
