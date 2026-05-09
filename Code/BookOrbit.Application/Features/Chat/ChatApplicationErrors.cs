namespace BookOrbit.Application.Features.Chat;
public static class ChatApplicationErrors
{
    static public readonly Error ChatGroupNotFoundById = ApplicationCommonErrors.NotFoundClass(ChatGroupErrors.ClassName, "Id", "Id");
    static public readonly Error StudentNotFound = ApplicationCommonErrors.NotFoundClass(StudentErrors.ClassName, "UserId", "User Id");
    static public readonly Error UserNotPartOfChatGroup = ApplicationCommonErrors.CustomUnauthorized(ChatGroupErrors.ClassName, "UserNotPartOfChatGroup", "You are not a participant in this chat group.");
        static public readonly Error ReceiverNotFound = ApplicationCommonErrors.NotFoundClass(StudentErrors.ClassName, "Id", "Receiver Id");
}
