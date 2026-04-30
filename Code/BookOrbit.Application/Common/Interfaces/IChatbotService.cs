namespace BookOrbit.Application.Common.Interfaces;

/// <summary>
/// Abstracts the LLM interaction so the Application layer is independent
/// of Semantic Kernel, OpenAI, or any other AI SDK.
/// </summary>
public interface IChatbotService
{
    /// <summary>
    /// Sends <paramref name="userMessage"/> together with the conversation
    /// <paramref name="history"/> to the underlying LLM and returns the assistant reply.
    /// </summary>
    Task<string> GetResponseAsync(
        string userId,
        string userMessage,
        IReadOnlyList<ChatMessage> history,
        CancellationToken ct = default);
}
