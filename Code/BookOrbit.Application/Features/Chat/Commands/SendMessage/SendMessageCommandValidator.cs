namespace BookOrbit.Application.Features.Chat.Commands.SendMessage;
public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ReceiverId)
            .Cascade(CascadeMode.Stop)
            .ReceiverIdRules();

        RuleFor(x => x.Content)
            .Cascade(CascadeMode.Stop)
            .MessageContentRules();

        RuleFor(x => x.SenderId)
    .Cascade(CascadeMode.Stop)
    .StudentIdRules();

    }
}
