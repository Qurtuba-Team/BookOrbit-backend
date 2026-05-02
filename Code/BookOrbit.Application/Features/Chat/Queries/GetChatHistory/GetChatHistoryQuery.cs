using BookOrbit.Application.Features.Chat.Dtos;

namespace BookOrbit.Application.Features.Chat.Queries.GetChatHistory;
public record GetChatHistoryQuery(
    Guid StudentId,
    Guid ChatGroupId,
    int Page,
    int PageSize) : ICachedQuery<Result<PaginatedList<ChatMessageDto>>>
{
    public string CacheKey => ChatCachingConstants.ChatHistoryKey(this);

    public string[] Tags => [ChatCachingConstants.ChatMessageTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(ChatCachingConstants.ExpirationInMinutes);
}
