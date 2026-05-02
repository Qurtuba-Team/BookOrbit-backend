using BookOrbit.Application.Common.Interfaces.ChatServices;

namespace BookOrbit.Application.Features.Chat.Commands.MarkMessagesAsRead;
public class MarkMessagesAsReadCommandHandler(
    ILogger<MarkMessagesAsReadCommandHandler> logger,
    IAppDbContext context,
    IChatService chatService,
    HybridCache cache) : IRequestHandler<MarkMessagesAsReadCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(MarkMessagesAsReadCommand command, CancellationToken ct)
    {


        var chatGroup = await context.ChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.Id == command.ChatGroupId, ct);

        if (chatGroup is null)
        {
            logger.LogWarning("ChatGroup {ChatGroupId} not found.", command.ChatGroupId);
            return ChatApplicationErrors.ChatGroupNotFoundById;
        }

        if (chatGroup.Student1Id != command.StudentId && chatGroup.Student2Id != command.StudentId)
        {
            logger.LogWarning("Student {StudentId} is not part of ChatGroup {ChatGroupId}.", command.StudentId, command.ChatGroupId);
            return ChatApplicationErrors.UserNotPartOfChatGroup;
        }

        var result = await chatService.MarkMessagesAsReadAsync(command.ChatGroupId, command.StudentId, ct);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to mark messages as read. Errors: {Errors}", string.Join(',', result.Errors));
            return result.Errors;
        }

        await cache.RemoveByTagAsync(ChatCachingConstants.ChatMessageTag, ct);

        logger.LogInformation("Messages marked as read in ChatGroup {ChatGroupId} by Student {StudentId}.",
            command.ChatGroupId, command.StudentId);

        return Result.Updated;
    }
}
