namespace BookOrbit.Infrastructure.Settings;

public class ChatbotSettings
{
    public string ModelId { get; set; } = "gpt-4o-mini";
    public int MaxHistoryMessages { get; set; } = 20;
    public int HistoryTtlMinutes { get; set; } = 30;
    public string SystemPrompt { get; set; } =
        "You are BookOrbit Assistant, a helpful library chatbot for university students. " +
        "You help users find books, check borrowing status, and understand their points balance. " +
        "When you need live data, call the available functions. " +
        "Be concise, friendly, and always respond in the same language the user uses.";
}
