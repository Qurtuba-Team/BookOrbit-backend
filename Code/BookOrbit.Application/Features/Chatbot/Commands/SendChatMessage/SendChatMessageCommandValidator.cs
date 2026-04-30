using FluentValidation;

namespace BookOrbit.Application.Features.Chatbot.Commands.SendChatMessage;

public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    public const int MaxMessageLength = 2000;

    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message must not be empty.")
            .MaximumLength(MaxMessageLength)
            .WithMessage($"Message must not exceed {MaxMessageLength} characters.");
    }
}
