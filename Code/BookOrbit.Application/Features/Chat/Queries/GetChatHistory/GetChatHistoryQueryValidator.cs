namespace BookOrbit.Application.Features.Chat.Queries.GetChatHistory;
public class GetChatHistoryQueryValidator : AbstractValidator<GetChatHistoryQuery>
{
    public GetChatHistoryQueryValidator()
    {
        RuleFor(x => x.ChatGroupId)
            .Cascade(CascadeMode.Stop)
            .ChatGroupIdRules();

        RuleFor(x => x.StudentId)
    .Cascade(CascadeMode.Stop)
    .StudentIdRules();

    }
}
