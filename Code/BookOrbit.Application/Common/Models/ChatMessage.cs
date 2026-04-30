namespace BookOrbit.Application.Common.Models;

/// <summary>
/// A single turn in a chatbot conversation.
/// Kept in the Application layer to decouple from any LLM SDK type.
/// </summary>
public record ChatMessage(ChatMessageRole Role, string Content);

public enum ChatMessageRole
{
    System,
    User,
    Assistant
}
