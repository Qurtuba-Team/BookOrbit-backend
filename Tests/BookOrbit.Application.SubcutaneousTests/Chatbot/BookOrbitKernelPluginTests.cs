namespace BookOrbit.Application.SubcutaneousTests.Chatbot;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Features.Books.Queries.GetBooks;
using BookOrbit.Application.Features.Students.Queries.GetStudentByUserId;
using BookOrbit.Domain.Common.Results;
using BookOrbit.Infrastructure.Services.Chatbot;
using FakeItEasy;
using FluentAssertions;
using MediatR;

/// <summary>
/// Unit tests for <see cref="BookOrbitKernelPlugin"/>.
/// ISender is faked so each tool is tested independently of the infrastructure.
/// </summary>
public class BookOrbitKernelPluginTests
{
    // ─────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────

    private static BookOrbitKernelPlugin CreatePlugin(
        ISender? sender = null,
        string userId = "user-1")
    {
        sender ??= A.Fake<ISender>();
        var currentUser = A.Fake<ICurrentUser>();
        A.CallTo(() => currentUser.Id).Returns(userId);
        return new BookOrbitKernelPlugin(sender, currentUser);
    }

    private static BookOrbitKernelPlugin CreatePluginWithUnauthenticatedUser(ISender? sender = null)
    {
        sender ??= A.Fake<ISender>();
        var currentUser = A.Fake<ICurrentUser>();
        A.CallTo(() => currentUser.Id).Returns(null);
        return new BookOrbitKernelPlugin(sender, currentUser);
    }

    // ─────────────────────────────────────────────────────────────
    //  SearchBooksAsync
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchBooksAsync_DispatchesGetBooksQueryToMediator()
    {
        GetBooksQuery? capturedQuery = null;
        var sender = A.Fake<ISender>();

        A.CallTo(() => sender.Send(A<IRequest<Result<PaginatedList<BookListItemDto>>>>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily((IRequest<Result<PaginatedList<BookListItemDto>>> req, CancellationToken _) =>
            {
                capturedQuery = req as GetBooksQuery;
                return Task.FromResult(Result<PaginatedList<BookListItemDto>>.Success(
                    new PaginatedList<BookListItemDto>([], 0, 1, 5)));
            });

        var plugin = CreatePlugin(sender);
        await plugin.SearchBooksAsync("Clean Code");

        capturedQuery.Should().NotBeNull();
        capturedQuery!.SearchTerm.Should().Be("Clean Code");
    }

    [Fact]
    public async Task SearchBooksAsync_WithCategory_ParsesCategoryCorrectly()
    {
        GetBooksQuery? capturedQuery = null;
        var sender = A.Fake<ISender>();

        A.CallTo(() => sender.Send(A<IRequest<Result<PaginatedList<BookListItemDto>>>>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily((IRequest<Result<PaginatedList<BookListItemDto>>> req, CancellationToken _) =>
            {
                capturedQuery = req as GetBooksQuery;
                return Task.FromResult(Result<PaginatedList<BookListItemDto>>.Success(
                    new PaginatedList<BookListItemDto>([], 0, 1, 5)));
            });

        var plugin = CreatePlugin(sender);
        await plugin.SearchBooksAsync("", "Science");

        capturedQuery!.Categories.Should().ContainSingle();
        capturedQuery.Categories![0].Should().Be(BookCategory.Science);
    }

    [Fact]
    public async Task SearchBooksAsync_NoBooksFound_ReturnsNoBooksFoundMessage()
    {
        var sender = A.Fake<ISender>();
        A.CallTo(() => sender.Send(A<IRequest<Result<PaginatedList<BookListItemDto>>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult(Result<PaginatedList<BookListItemDto>>.Success(
                new PaginatedList<BookListItemDto>([], 0, 1, 5))));

        var plugin = CreatePlugin(sender);
        var result = await plugin.SearchBooksAsync("xyzzy-nonexistent");

        result.Should().Contain("No books were found");
    }

    [Fact]
    public async Task SearchBooksAsync_QueryFails_ReturnsNoBooksFoundMessage()
    {
        var sender = A.Fake<ISender>();
        A.CallTo(() => sender.Send(A<IRequest<Result<PaginatedList<BookListItemDto>>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.FromResult(Result<PaginatedList<BookListItemDto>>.Failure(
                Error.NotFound("Books.NotFound", "No books found"))));

        var plugin = CreatePlugin(sender);
        var result = await plugin.SearchBooksAsync("anything");

        result.Should().Contain("No books were found");
    }

    // ─────────────────────────────────────────────────────────────
    //  GetMyPointsAsync
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyPointsAsync_DispatchesGetStudentByUserIdQuery()
    {
        GetStudentByUserIdQuery? capturedQuery = null;
        var sender = A.Fake<ISender>();

        A.CallTo(() => sender.Send(
                A<IRequest<Result<StudentDtoWithContactInfo>>>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((IRequest<Result<StudentDtoWithContactInfo>> req, CancellationToken _) =>
            {
                capturedQuery = req as GetStudentByUserIdQuery;
                return Task.FromResult(Result<StudentDtoWithContactInfo>.Failure(
                    Error.NotFound("x", "y")));
            });

        var plugin = CreatePlugin(sender, "user-42");
        await plugin.GetMyPointsAsync();

        capturedQuery.Should().NotBeNull();
        capturedQuery!.UserId.Should().Be("user-42");
    }

    [Fact]
    public async Task GetMyPointsAsync_UnauthenticatedUser_ReturnsNotIdentifiedMessage()
    {
        var plugin = CreatePluginWithUnauthenticatedUser();
        var result = await plugin.GetMyPointsAsync();
        result.Should().Contain("could not identify");
    }

    [Fact]
    public async Task GetMyPointsAsync_QueryFails_ReturnsCouldNotRetrieveMessage()
    {
        var sender = A.Fake<ISender>();
        A.CallTo(() => sender.Send(
                A<IRequest<Result<StudentDtoWithContactInfo>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult(Result<StudentDtoWithContactInfo>.Failure(
                Error.NotFound("Student.NotFound", "Not found"))));

        var plugin = CreatePlugin(sender);
        var result = await plugin.GetMyPointsAsync();

        result.Should().Contain("could not retrieve");
    }

    // ─────────────────────────────────────────────────────────────
    //  GetMyProfileAsync
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyProfileAsync_UnauthenticatedUser_ReturnsNotIdentifiedMessage()
    {
        var plugin = CreatePluginWithUnauthenticatedUser();
        var result = await plugin.GetMyProfileAsync();
        result.Should().Contain("could not identify");
    }

    [Fact]
    public async Task GetMyProfileAsync_QueryFails_ReturnsCouldNotRetrieveMessage()
    {
        var sender = A.Fake<ISender>();
        A.CallTo(() => sender.Send(
                A<IRequest<Result<StudentDtoWithContactInfo>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult(Result<StudentDtoWithContactInfo>.Failure(
                Error.NotFound("Student.NotFound", "Not found"))));

        var plugin = CreatePlugin(sender);
        var result = await plugin.GetMyProfileAsync();

        result.Should().Contain("could not retrieve");
    }
}
