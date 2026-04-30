namespace BookOrbit.Application.Common.Interfaces;

/// <summary>
/// Persists and retrieves per-user conversation history without coupling
/// the Application layer to a specific storage mechanism.
/// </summary>
public interface IConversationHistoryService
{
    Task<List<ChatMessage>> LoadAsync(string userId, CancellationToken ct = default);
    Task SaveAsync(string userId, List<ChatMessage> messages, CancellationToken ct = default);
}
