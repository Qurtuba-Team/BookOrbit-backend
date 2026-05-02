using BookOrbit.Domain.ChatGroups;
using BookOrbit.Domain.Students;

namespace BookOrbit.Domain.ChatMessages;

public class ChatMessage : AuditableEntity
{
    public string Content { get; private set; } = null!;
    public Guid SenderId { get; }
    public Guid ChatGroupId { get; }
    public bool IsRead { get; private set; }

    public Student? Sender { get; private set; }
    public ChatGroup? ChatGroup { get; private set; }

    public const int ContentMaxLength = 2000;

    private ChatMessage() { }

    private ChatMessage(Guid id, string content, Guid senderId, Guid chatGroupId) : base(id)
    {
        Content = content;
        SenderId = senderId;
        ChatGroupId = chatGroupId;
        IsRead = false;
    }

    public static Result<ChatMessage> Create(Guid id, string content, Guid senderId, Guid chatGroupId)
    {
        if (id == Guid.Empty)
            return ChatMessageErrors.IdRequired;

        if (string.IsNullOrWhiteSpace(content))
            return ChatMessageErrors.ContentRequired;

        if(content.Length > ContentMaxLength)
            return ChatMessageErrors.ContentTooLong;

        if (senderId == Guid.Empty)
            return ChatMessageErrors.SenderIdRequired;

        if (chatGroupId == Guid.Empty)
            return ChatMessageErrors.ChatGroupIdRequired;

        return new ChatMessage(id, content, senderId, chatGroupId);
    }

    public Result<Updated> MarkAsRead()
    {
        IsRead = true;
        return Result.Updated;
    }
}
