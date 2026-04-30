using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using BookOrbit.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

#pragma warning disable SKEXP0070 // Google AI connector is experimental in SK 1.30.0

namespace BookOrbit.Infrastructure.Services.Chatbot;

public class SemanticKernelChatbotService(
    Kernel kernel,
    IOptions<ChatbotSettings> chatbotSettings,
    ILogger<SemanticKernelChatbotService> logger) : IChatbotService
{
    private readonly ChatbotSettings _settings = chatbotSettings.Value;

    public async Task<string> GetResponseAsync(
        string userId,
        string userMessage,
        IReadOnlyList<ChatMessage> history,
        CancellationToken ct = default)
    {
        // Build Semantic Kernel ChatHistory from our LLM-agnostic ChatMessage list.
        var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory(_settings.SystemPrompt);

        foreach (var msg in history)
        {
            switch (msg.Role)
            {
                case ChatMessageRole.User:
                    chatHistory.AddUserMessage(msg.Content);
                    break;
                case ChatMessageRole.Assistant:
                    chatHistory.AddAssistantMessage(msg.Content);
                    break;
                // System messages are already set via the constructor above.
            }
        }

        chatHistory.AddUserMessage(userMessage);

        var executionSettings = new GeminiPromptExecutionSettings
        {
            // Auto-invoke: SK calls our KernelPlugin functions automatically
            // when the LLM requests them via function calling.
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        try
        {
            var result = await chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                kernel,
                ct);

            return result.Content ?? "I'm sorry, I couldn't generate a response.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "SemanticKernel: LLM call failed for user {UserId}.", userId);
            throw; // Let the handler map this to ChatbotApplicationErrors.ChatbotUnavailable.
        }
    }
}
