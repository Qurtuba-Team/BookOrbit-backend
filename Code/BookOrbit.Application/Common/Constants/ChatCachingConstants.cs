
namespace BookOrbit.Application.Common.Constants;
public class ChatCachingConstants
{
    public const string ChatGroupTag = "chatgroup";
    public const string ChatMessageTag = "chatmessage";

    public static string ChatGroupKey(Guid chatGroupId) => $"chatgroup:{chatGroupId}";

    public static string ChatGroupListKey(GetUserChatGroupsQuery query)
        =>
        $"chatgroups:sid={query.StudentId}:p={query.Page}:ps={query.PageSize}";

    public static string ChatHistoryKey(GetChatHistoryQuery query)
        =>
        $"chathistory:gid={query.ChatGroupId}:p={query.Page}:ps={query.PageSize}";

    public const int ExpirationInMinutes = 5;
}
