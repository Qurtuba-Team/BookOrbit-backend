using BookOrbit.Application.Common.Interfaces.ChatServices;
using BookOrbit.Application.Features.Chat.Dtos;

namespace BookOrbit.Application.Features.Chat.Commands.SendMessage;
public class SendMessageCommandHandler(
    ILogger<SendMessageCommandHandler> logger,
    IAppDbContext context,
    IChatService chatService,
    ICurrentUser currentUser,
    HybridCache cache) : IRequestHandler<SendMessageCommand, Result<ChatMessageDto>>
{
    public async Task<Result<ChatMessageDto>> Handle(SendMessageCommand command, CancellationToken ct)
    {
        var userId = currentUser.Id;

        if (string.IsNullOrEmpty(userId))
            return ChatApplicationErrors.StudentNotFound;

        var senderId = await context.Students
            .Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(ct);

        if (senderId == Guid.Empty)
        {
            logger.LogWarning("Student not found for UserId: {UserId}", userId);
            return ChatApplicationErrors.StudentNotFound;
        }

        var result = await chatService.SaveMessageAsync(senderId, command.ReceiverId, command.Content, ct);

        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send message. Errors: {Errors}", string.Join(',', result.Errors));
            return result.Errors;
        }

        await cache.RemoveByTagAsync(ChatCachingConstants.ChatMessageTag, ct);
        await cache.RemoveByTagAsync(ChatCachingConstants.ChatGroupTag, ct);

        logger.LogInformation("Message {MessageId} sent from {SenderId} to {ReceiverId}",
            result.Value.Id, senderId, command.ReceiverId);

        return ChatMessageDto.FromEntity(result.Value);
    }
}
