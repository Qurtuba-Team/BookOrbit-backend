using BookOrbit.Application.Features.Chat.Dtos;

namespace BookOrbit.Application.Features.Chat.Queries.GetUserChatGroups;
public record GetUserChatGroupsQuery(
    Guid StudentId,
    int Page,
    int PageSize) : ICachedQuery<Result<PaginatedList<ChatGroupListItemDto>>>
{
    public string CacheKey => ChatCachingConstants.ChatGroupListKey(this);

    public string[] Tags => [ChatCachingConstants.ChatGroupTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(ChatCachingConstants.ExpirationInMinutes);
}
