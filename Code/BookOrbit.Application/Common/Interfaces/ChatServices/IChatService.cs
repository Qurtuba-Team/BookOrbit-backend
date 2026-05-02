namespace BookOrbit.Application.Common.Interfaces.ChatServices;

public interface IChatService
{
    Task<Result<ChatGroup>> GetOrCreateChatGroupAsync(Guid student1Id, Guid student2Id, CancellationToken cancellationToken = default);
    Task<Result<ChatMessage>> SaveMessageAsync(Guid senderId, Guid receiverId, string content, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatMessage>>> GetChatHistoryAsync(Guid chatGroupId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroup>>> GetUserChatGroupsAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<Result<Updated>> MarkMessagesAsReadAsync(Guid chatGroupId, Guid studentId, CancellationToken cancellationToken = default);
}
