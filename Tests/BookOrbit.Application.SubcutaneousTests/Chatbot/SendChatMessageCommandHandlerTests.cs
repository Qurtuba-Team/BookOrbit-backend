namespace BookOrbit.Application.SubcutaneousTests.Chatbot;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Common.Models;
using BookOrbit.Application.Features.Chatbot;
using BookOrbit.Application.Features.Chatbot.Commands.SendChatMessage;
using BookOrbit.Application.Features.Chatbot.Dtos;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Unit tests for <see cref="SendChatMessageCommandHandler"/>.
/// All external dependencies are faked so we test the handler's logic in isolation.
/// </summary>
public class SendChatMessageCommandHandlerTests
{
    // ─────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────

    private static SendChatMessageCommandHandler CreateHandler(
        ICurrentUser? currentUser = null,
        IChatbotService? chatbotService = null,
        IConversationHistoryService? historyService = null)
    {
        currentUser ??= CreateAuthenticatedUser("user-1");
        chatbotService ??= CreateDefaultChatbotService("Hello!");
        historyService ??= CreateEmptyHistoryService();

        return new SendChatMessageCommandHandler(
            currentUser,
            chatbotService,
            historyService,
            NullLogger<SendChatMessageCommandHandler>.Instance);
    }

    private static ICurrentUser CreateAuthenticatedUser(string userId)
    {
        var user = A.Fake<ICurrentUser>();
        A.CallTo(() => user.Id).Returns(userId);
        return user;
    }

    private static ICurrentUser CreateUnauthenticatedUser()
    {
        var user = A.Fake<ICurrentUser>();
        A.CallTo(() => user.Id).Returns(null);
        return user;
    }

    private static IChatbotService CreateDefaultChatbotService(string reply)
    {
        var service = A.Fake<IChatbotService>();
        A.CallTo(() => service.GetResponseAsync(
                A<string>.Ignored,
                A<string>.Ignored,
                A<IReadOnlyList<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult(reply));
        return service;
    }

    private static IConversationHistoryService CreateEmptyHistoryService()
    {
        var history = A.Fake<IConversationHistoryService>();
        A.CallTo(() => history.LoadAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult(new List<ChatMessage>()));
        return history;
    }

    private static IConversationHistoryService CreateHistoryServiceWithMessages(List<ChatMessage> messages)
    {
        var history = A.Fake<IConversationHistoryService>();
        A.CallTo(() => history.LoadAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult(messages));
        return history;
    }

    // ─────────────────────────────────────────────────────────────
    //  Happy Paths
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidMessage_ReturnsSuccessWithReply()
    {
        const string expectedReply = "Here are some science books!";
        var handler = CreateHandler(chatbotService: CreateDefaultChatbotService(expectedReply));

        var result = await handler.Handle(new SendChatMessageCommand("Find me a science book"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Reply.Should().Be(expectedReply);
        result.Value.SentAt.Should().BeCloseTo(DateTimeOffset.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidMessage_CallsChatbotServiceExactlyOnce()
    {
        var chatbotService = CreateDefaultChatbotService("OK");
        var handler = CreateHandler(chatbotService: chatbotService);

        await handler.Handle(new SendChatMessageCommand("Hello"), default);

        A.CallTo(() => chatbotService.GetResponseAsync(
                A<string>.Ignored,
                "Hello",
                A<IReadOnlyList<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_ValidMessage_PassesUserIdToChatbotService()
    {
        const string userId = "user-42";
        var chatbotService = CreateDefaultChatbotService("response");
        var handler = CreateHandler(
            currentUser: CreateAuthenticatedUser(userId),
            chatbotService: chatbotService);

        await handler.Handle(new SendChatMessageCommand("Hello"), default);

        A.CallTo(() => chatbotService.GetResponseAsync(
                userId,
                A<string>.Ignored,
                A<IReadOnlyList<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_ValidMessage_LoadsAndPassesHistoryToChatbotService()
    {
        var history = new List<ChatMessage>
        {
            new(ChatMessageRole.User, "What books do you have?"),
            new(ChatMessageRole.Assistant, "We have many books.")
        };
        IReadOnlyList<ChatMessage>? capturedHistory = null;
        var chatbotService = A.Fake<IChatbotService>();
        A.CallTo(() => chatbotService.GetResponseAsync(
                A<string>.Ignored,
                A<string>.Ignored,
                A<IReadOnlyList<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, string __, IReadOnlyList<ChatMessage> hist, CancellationToken ___) =>
            {
                capturedHistory = hist;
                return Task.FromResult("More books!");
            });

        var handler = CreateHandler(
            historyService: CreateHistoryServiceWithMessages(history),
            chatbotService: chatbotService);

        await handler.Handle(new SendChatMessageCommand("Tell me more"), default);

        capturedHistory.Should().HaveCount(2);
        capturedHistory![0].Content.Should().Be("What books do you have?");
    }

    [Fact]
    public async Task Handle_ValidMessage_SavesHistoryWithBothTurnsAppended()
    {
        var historyService = CreateEmptyHistoryService();
        List<ChatMessage>? savedMessages = null;
        A.CallTo(() => historyService.SaveAsync(
                A<string>.Ignored,
                A<List<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, List<ChatMessage> msgs, CancellationToken __) =>
            {
                savedMessages = msgs;
                return Task.CompletedTask;
            });

        var handler = CreateHandler(
            chatbotService: CreateDefaultChatbotService("I found some books!"),
            historyService: historyService);

        await handler.Handle(new SendChatMessageCommand("Find books"), default);

        savedMessages.Should().HaveCount(2);
        savedMessages![0].Role.Should().Be(ChatMessageRole.User);
        savedMessages[0].Content.Should().Be("Find books");
        savedMessages[1].Role.Should().Be(ChatMessageRole.Assistant);
        savedMessages[1].Content.Should().Be("I found some books!");
    }

    [Fact]
    public async Task Handle_ExistingHistory_AppendsToBothTurnsAndSavesAll()
    {
        var existingHistory = new List<ChatMessage>
        {
            new(ChatMessageRole.User, "Hello"),
            new(ChatMessageRole.Assistant, "Hi there!")
        };
        List<ChatMessage>? savedMessages = null;
        var historyService = CreateHistoryServiceWithMessages(existingHistory);
        A.CallTo(() => historyService.SaveAsync(
                A<string>.Ignored,
                A<List<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, List<ChatMessage> msgs, CancellationToken __) =>
            {
                savedMessages = msgs;
                return Task.CompletedTask;
            });

        var handler = CreateHandler(
            chatbotService: CreateDefaultChatbotService("3 books found."),
            historyService: historyService);

        await handler.Handle(new SendChatMessageCommand("Search for books"), default);

        savedMessages.Should().HaveCount(4,
            because: "2 pre-existing messages + 1 user turn + 1 assistant turn");
    }

    // ─────────────────────────────────────────────────────────────
    //  Authentication
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UnauthenticatedUser_ReturnsUserNotFoundError()
    {
        var handler = CreateHandler(currentUser: CreateUnauthenticatedUser());

        var result = await handler.Handle(new SendChatMessageCommand("Find books"), default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(ChatbotApplicationErrors.UserNotFound);
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_DoesNotCallChatbotService()
    {
        var chatbotService = CreateDefaultChatbotService("response");
        var handler = CreateHandler(
            currentUser: CreateUnauthenticatedUser(),
            chatbotService: chatbotService);

        await handler.Handle(new SendChatMessageCommand("Hello"), default);

        A.CallTo(() => chatbotService.GetResponseAsync(
                A<string>.Ignored,
                A<string>.Ignored,
                A<IReadOnlyList<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    // ─────────────────────────────────────────────────────────────
    //  Error Handling
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ChatbotServiceThrowsException_ReturnsChatbotUnavailableError()
    {
        var chatbotService = A.Fake<IChatbotService>();
        A.CallTo(() => chatbotService.GetResponseAsync(
                A<string>.Ignored,
                A<string>.Ignored,
                A<IReadOnlyList<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .Throws(new HttpRequestException("LLM endpoint unreachable"));

        var handler = CreateHandler(chatbotService: chatbotService);
        var result = await handler.Handle(new SendChatMessageCommand("Hello"), default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(ChatbotApplicationErrors.ChatbotUnavailable);
    }

    [Fact]
    public async Task Handle_ChatbotServiceThrowsException_DoesNotSaveHistory()
    {
        var historyService = CreateEmptyHistoryService();
        var chatbotService = A.Fake<IChatbotService>();
        A.CallTo(() => chatbotService.GetResponseAsync(
                A<string>.Ignored,
                A<string>.Ignored,
                A<IReadOnlyList<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .Throws(new InvalidOperationException("timeout"));

        var handler = CreateHandler(chatbotService: chatbotService, historyService: historyService);
        await handler.Handle(new SendChatMessageCommand("Hello"), default);

        A.CallTo(() => historyService.SaveAsync(
                A<string>.Ignored,
                A<List<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var chatbotService = A.Fake<IChatbotService>();
        A.CallTo(() => chatbotService.GetResponseAsync(
                A<string>.Ignored,
                A<string>.Ignored,
                A<IReadOnlyList<ChatMessage>>.Ignored,
                A<CancellationToken>.Ignored))
            .Throws<OperationCanceledException>();

        var handler = CreateHandler(chatbotService: chatbotService);

        Func<Task> act = () => handler.Handle(new SendChatMessageCommand("Hello"), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>(
            because: "cancellation must never be swallowed as a ChatbotUnavailable result");
    }
}
