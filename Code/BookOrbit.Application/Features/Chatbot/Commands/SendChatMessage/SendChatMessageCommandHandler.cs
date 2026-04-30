using Microsoft.Extensions.Logging;

namespace BookOrbit.Application.Features.Chatbot.Commands.SendChatMessage;

public class SendChatMessageCommandHandler(
    ICurrentUser currentUser,
    IChatbotService chatbotService,
    IConversationHistoryService historyService,
    ILogger<SendChatMessageCommandHandler> logger)
    : IRequestHandler<SendChatMessageCommand, Result<ChatbotResponseDto>>
{
    public async Task<Result<ChatbotResponseDto>> Handle(SendChatMessageCommand command, CancellationToken ct)
    {
        var userId = currentUser.Id;
        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("Chatbot: unauthenticated request rejected.");
            return ChatbotApplicationErrors.UserNotFound;
        }

        // 1. Load conversation history from cache.
        var history = await historyService.LoadAsync(userId, ct);

        // 2. Call the LLM; the service handles function calling internally.
        string reply;
        try
        {
            reply = await chatbotService.GetResponseAsync(userId, command.Message, history, ct);
        }
        catch (OperationCanceledException)
        {
            throw; // Propagate cancellation correctly.
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Chatbot: unexpected error while processing message for user {UserId}.", userId);
            return ChatbotApplicationErrors.ChatbotUnavailable;
        }

        // 3. Append both turns to history and persist.
        history.Add(new ChatMessage(ChatMessageRole.User, command.Message));
        history.Add(new ChatMessage(ChatMessageRole.Assistant, reply));
        // Use CancellationToken.None: the LLM already produced a reply — persist the turn
        // regardless of whether the client disconnected, so history stays consistent.
        await historyService.SaveAsync(userId, history, CancellationToken.None);

        logger.LogInformation("Chatbot: message processed successfully for user {UserId}.", userId);

        return new ChatbotResponseDto(reply, DateTimeOffset.UtcNow);
    }
}
