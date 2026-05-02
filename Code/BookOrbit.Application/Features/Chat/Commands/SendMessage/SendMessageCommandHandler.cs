using BookOrbit.Application.Common.Interfaces.ChatServices;

namespace BookOrbit.Application.Features.Chat.Commands.SendMessage;
public class SendMessageCommandHandler(
    ILogger<SendMessageCommandHandler> logger,
    IChatService chatService,
    HybridCache cache) : IRequestHandler<SendMessageCommand, Result<ChatMessageDto>>
{
    public async Task<Result<ChatMessageDto>> Handle(SendMessageCommand command, CancellationToken ct)
    {
        var result = await chatService.SaveMessageAsync(command.SenderId, command.ReceiverId, command.Content, ct);

        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send message. Errors: {Errors}", string.Join(',', result.Errors));
            return result.Errors;
        }

        await cache.RemoveByTagAsync(ChatCachingConstants.ChatMessageTag, ct);
        await cache.RemoveByTagAsync(ChatCachingConstants.ChatGroupTag, ct);

        logger.LogInformation("Message {MessageId} sent from {SenderId} to {ReceiverId}",
            result.Value.Id, command.SenderId, command.ReceiverId);

        return ChatMessageDto.FromEntity(result.Value);
    }
}
