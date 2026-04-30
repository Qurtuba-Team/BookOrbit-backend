using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Common.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using BookOrbit.Infrastructure.Settings;

namespace BookOrbit.Infrastructure.Services.Chatbot;

public class DistributedCacheConversationHistoryService(
    IDistributedCache cache,
    IOptions<ChatbotSettings> chatbotSettings) : IConversationHistoryService
{
    private readonly ChatbotSettings _settings = chatbotSettings.Value;

    private static string CacheKey(string userId) => $"chatbot:history:{userId}";

    public async Task<List<ChatMessage>> LoadAsync(string userId, CancellationToken ct = default)
    {
        var bytes = await cache.GetAsync(CacheKey(userId), ct);
        if (bytes is null || bytes.Length == 0)
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<ChatMessage>>(bytes) ?? [];
        }
        catch (JsonException)
        {
            // Stale / corrupted cache entry — start fresh.
            return [];
        }
    }

    public async Task SaveAsync(string userId, List<ChatMessage> messages, CancellationToken ct = default)
    {
        // Enforce sliding window — drop oldest messages beyond the cap.
        var capped = messages.Count > _settings.MaxHistoryMessages
            ? messages[^_settings.MaxHistoryMessages..]
            : messages;

        var bytes = JsonSerializer.SerializeToUtf8Bytes(capped);

        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(_settings.HistoryTtlMinutes),
        };

        await cache.SetAsync(CacheKey(userId), bytes, options, ct);
    }
}
