namespace BookOrbit.Application.SubcutaneousTests.Chatbot;

using BookOrbit.Application.Common.Models;
using BookOrbit.Infrastructure.Services.Chatbot;
using BookOrbit.Infrastructure.Settings;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

/// <summary>
/// Unit tests for <see cref="DistributedCacheConversationHistoryService"/>.
/// IDistributedCache is faked to avoid needing a real Redis instance.
/// </summary>
public class DistributedCacheConversationHistoryServiceTests
{
    // ─────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────

    private static DistributedCacheConversationHistoryService CreateService(
        IDistributedCache? cache = null,
        int maxMessages = 20,
        int ttlMinutes = 30)
    {
        cache ??= A.Fake<IDistributedCache>();
        var settings = new ChatbotSettings
        {
            MaxHistoryMessages = maxMessages,
            HistoryTtlMinutes = ttlMinutes
        };
        return new DistributedCacheConversationHistoryService(
            cache,
            Options.Create(settings));
    }

    private static byte[] Serialize(List<ChatMessage> messages) =>
        JsonSerializer.SerializeToUtf8Bytes(messages);

    // ─────────────────────────────────────────────────────────────
    //  LoadAsync
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_CacheEmpty_ReturnsEmptyList()
    {
        var cache = A.Fake<IDistributedCache>();
        A.CallTo(() => cache.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<byte[]?>(null));

        var service = CreateService(cache);

        var result = await service.LoadAsync("user-1");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_CacheHasMessages_ReturnsDeserializedMessages()
    {
        var messages = new List<ChatMessage>
        {
            new(ChatMessageRole.User, "Hello"),
            new(ChatMessageRole.Assistant, "Hi there!")
        };
        var cache = A.Fake<IDistributedCache>();
        A.CallTo(() => cache.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<byte[]?>(Serialize(messages)));

        var service = CreateService(cache);
        var result = await service.LoadAsync("user-1");

        result.Should().HaveCount(2);
        result[0].Content.Should().Be("Hello");
        result[1].Content.Should().Be("Hi there!");
    }

    [Fact]
    public async Task LoadAsync_CacheHasCorruptedData_ReturnsEmptyListWithoutThrowing()
    {
        var cache = A.Fake<IDistributedCache>();
        A.CallTo(() => cache.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<byte[]?>(Encoding.UTF8.GetBytes("{this is not valid json{{{")));

        var service = CreateService(cache);

        Func<Task<List<ChatMessage>>> act = () => service.LoadAsync("user-1");

        var result = await act.Should().NotThrowAsync();
        result.Subject.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_UsesCorrectCacheKeyForUser()
    {
        string? capturedKey = null;
        var cache = A.Fake<IDistributedCache>();
        A.CallTo(() => cache.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily((string key, CancellationToken _) =>
            {
                capturedKey = key;
                return Task.FromResult<byte[]?>(null);
            });

        var service = CreateService(cache);
        await service.LoadAsync("user-99");

        capturedKey.Should().Be("chatbot:history:user-99");
    }

    // ─────────────────────────────────────────────────────────────
    //  SaveAsync — window capping
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_MessagesWithinLimit_SavesAllMessages()
    {
        List<ChatMessage>? savedMessages = null;
        var cache = A.Fake<IDistributedCache>();
        A.CallTo(() => cache.SetAsync(
                A<string>.Ignored,
                A<byte[]>.Ignored,
                A<DistributedCacheEntryOptions>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, byte[] bytes, DistributedCacheEntryOptions __, CancellationToken ___) =>
            {
                savedMessages = JsonSerializer.Deserialize<List<ChatMessage>>(bytes);
                return Task.CompletedTask;
            });

        var service = CreateService(cache, maxMessages: 5);
        var messages = Enumerable.Range(1, 4)
            .Select(i => new ChatMessage(ChatMessageRole.User, $"Message {i}"))
            .ToList();

        await service.SaveAsync("user-1", messages);

        savedMessages.Should().HaveCount(4);
    }

    [Fact]
    public async Task SaveAsync_MessagesExceedLimit_TrimsOldestMessages()
    {
        List<ChatMessage>? savedMessages = null;
        var cache = A.Fake<IDistributedCache>();
        A.CallTo(() => cache.SetAsync(
                A<string>.Ignored,
                A<byte[]>.Ignored,
                A<DistributedCacheEntryOptions>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, byte[] bytes, DistributedCacheEntryOptions __, CancellationToken ___) =>
            {
                savedMessages = JsonSerializer.Deserialize<List<ChatMessage>>(bytes);
                return Task.CompletedTask;
            });

        var service = CreateService(cache, maxMessages: 3);
        var messages = Enumerable.Range(1, 5)
            .Select(i => new ChatMessage(ChatMessageRole.User, $"Message {i}"))
            .ToList();

        await service.SaveAsync("user-1", messages);

        savedMessages.Should().HaveCount(3,
            because: "only the 3 most recent messages should be kept");
        savedMessages![0].Content.Should().Be("Message 3",
            because: "oldest 2 messages should have been dropped");
        savedMessages[2].Content.Should().Be("Message 5");
    }

    [Fact]
    public async Task SaveAsync_SetsSlidingExpirationFromSettings()
    {
        DistributedCacheEntryOptions? capturedOptions = null;
        var cache = A.Fake<IDistributedCache>();
        A.CallTo(() => cache.SetAsync(
                A<string>.Ignored,
                A<byte[]>.Ignored,
                A<DistributedCacheEntryOptions>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, byte[] __, DistributedCacheEntryOptions opts, CancellationToken ___) =>
            {
                capturedOptions = opts;
                return Task.CompletedTask;
            });

        var service = CreateService(cache, ttlMinutes: 45);
        await service.SaveAsync("user-1", [new(ChatMessageRole.User, "Test")]);

        capturedOptions.Should().NotBeNull();
        capturedOptions!.SlidingExpiration.Should().Be(TimeSpan.FromMinutes(45));
    }

    [Fact]
    public async Task SaveAsync_UsesCorrectCacheKeyForUser()
    {
        string? capturedKey = null;
        var cache = A.Fake<IDistributedCache>();
        A.CallTo(() => cache.SetAsync(
                A<string>.Ignored,
                A<byte[]>.Ignored,
                A<DistributedCacheEntryOptions>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string key, byte[] __, DistributedCacheEntryOptions ___, CancellationToken ____) =>
            {
                capturedKey = key;
                return Task.CompletedTask;
            });

        var service = CreateService(cache);
        await service.SaveAsync("special-user-77", [new(ChatMessageRole.User, "Hello")]);

        capturedKey.Should().Be("chatbot:history:special-user-77");
    }
}
