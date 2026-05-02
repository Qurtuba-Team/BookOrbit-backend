namespace BookOrbit.Domain.ChatMessages;

public static class ChatMessageErrors
{
    private const string ClassName = nameof(ChatMessage);

    public static readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    public static readonly Error ContentRequired = DomainCommonErrors.RequiredProp(ClassName, "Content", "Content");
    public static readonly Error ContentTooLong = DomainCommonErrors.MaxLengthProp(ClassName, "Content", "Content", ChatMessage.ContentMaxLength);
    public static readonly Error SenderIdRequired = DomainCommonErrors.RequiredProp(ClassName, "SenderId", "Sender Id");
    public static readonly Error ChatGroupIdRequired = DomainCommonErrors.RequiredProp(ClassName, "ChatGroupId", "Chat Group Id");
}
