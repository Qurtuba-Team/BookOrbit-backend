using BookOrbit.Domain.ChatGroups;
using BookOrbit.Domain.ChatMessages;

namespace BookOrbit.Application.Features.Chat;
public static class ChatApplicationErrors
{
    private const string ChatGroupClassName = nameof(ChatGroup);
    private const string ChatMessageClassName = nameof(ChatMessage);

    static public readonly Error ChatGroupNotFoundById = ApplicationCommonErrors.NotFoundClass(ChatGroupClassName, "Id", "Id");
    static public readonly Error StudentNotFound = ApplicationCommonErrors.NotFoundClass("Student", "UserId", "User Id");
    static public readonly Error UserNotPartOfChatGroup = ApplicationCommonErrors.CustomUnauthorized(ChatGroupClassName, "UserNotPartOfChatGroup", "You are not a participant in this chat group.");
}
