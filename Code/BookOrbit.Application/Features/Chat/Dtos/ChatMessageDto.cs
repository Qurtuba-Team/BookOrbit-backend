using BookOrbit.Domain.ChatMessages;

namespace BookOrbit.Application.Features.Chat.Dtos;
public record ChatMessageDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid SenderId { get; set; } = Guid.Empty;
    public Guid ChatGroupId { get; set; } = Guid.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonConstructor]
    private ChatMessageDto() { }

    public ChatMessageDto(
        Guid id,
        string content,
        Guid senderId,
        Guid chatGroupId,
        bool isRead,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Content = content;
        SenderId = senderId;
        ChatGroupId = chatGroupId;
        IsRead = isRead;
        CreatedAtUtc = createdAtUtc;
    }

    static public ChatMessageDto FromEntity(ChatMessage message)
        =>
        new(
            message.Id,
            message.Content,
            message.SenderId,
            message.ChatGroupId,
            message.IsRead,
            message.CreatedAtUtc);
}
