namespace BookOrbit.Application.Features.Chat.Commands.MarkMessagesAsRead;
public class MarkMessagesAsReadCommandValidator : AbstractValidator<MarkMessagesAsReadCommand>
{
    public MarkMessagesAsReadCommandValidator()
    {
        RuleFor(x => x.ChatGroupId)
            .Cascade(CascadeMode.Stop)
            .ChatGroupIdRules();

        RuleFor(x=>x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}
