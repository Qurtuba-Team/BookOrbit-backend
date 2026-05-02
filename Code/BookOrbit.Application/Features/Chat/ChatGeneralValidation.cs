using BookOrbit.Domain.ChatGroups;
using BookOrbit.Domain.ChatMessages;

namespace BookOrbit.Application.Features.Chat;
static public class ChatGeneralValidation
{
    static public IRuleBuilder<T, Guid> ChatGroupIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
    ruleBuilder
        .NotEmpty().WithMessage(ChatGroupErrors.IdRequired.Description)
        .Must(id => id != Guid.Empty).WithMessage(ChatGroupErrors.IdRequired.Description);

    static public IRuleBuilder<T, Guid> ReceiverIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
    ruleBuilder
        .NotEmpty().WithMessage(ChatMessageErrors.SenderIdRequired.Description)
        .Must(id => id != Guid.Empty).WithMessage(ChatMessageErrors.SenderIdRequired.Description);

    static public IRuleBuilder<T, string> MessageContentRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
    ruleBuilder
        .NotEmpty().WithMessage(ChatMessageErrors.ContentRequired.Description)
        .MaximumLength(ChatMessage.ContentMaxLength).WithMessage(ChatMessageErrors.ContentTooLong.Description);
}
