namespace BookOrbit.Application.SubcutaneousTests.Books.Services;

using System.Net;
using System.Text;
using BookOrbit.Application.Common.Constants;
using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Infrastructure.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Unit tests for <see cref="BookCoverRetrievalService"/>.
/// HttpClient is controlled via a delegating stub so we do not hit real endpoints.
/// IAppCache is faked to isolate HTTP behaviour from caching behaviour.
/// </summary>
public class BookCoverRetrievalServiceTests
{
    // ─────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an <see cref="HttpClient"/> whose inner handler always returns
    /// the given <paramref name="response"/> for every request.
    /// </summary>
    private static HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .Returns(Task.FromResult(response));

        return new HttpClient(handler);
    }

    /// <summary>
    /// Creates an <see cref="IAppCache"/> that delegates straight to the factory
    /// (i.e., never caches) — isolating HTTP behaviour from caching.
    /// </summary>
    private static IAppCache CreatePassThroughCache()
    {
        var cache = A.Fake<IAppCache>();

        A.CallTo(() => cache.GetOrCreateAsync(
                A<string>.Ignored,
                A<Func<CancellationToken, ValueTask<string>>>.Ignored,
                A<TimeSpan>.Ignored,
                A<IEnumerable<string>>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, Func<CancellationToken, ValueTask<string>> factory,
                TimeSpan __, IEnumerable<string> ___, CancellationToken ct) =>
                factory(ct).AsTask());

        return cache;
    }

    /// <summary>
    /// Creates an <see cref="IAppCache"/> that remembers one fixed value
    /// (as if the result was already cached).
    /// </summary>
    private static IAppCache CreatePreloadedCache(string cachedValue)
    {
        var cache = A.Fake<IAppCache>();

        A.CallTo(() => cache.GetOrCreateAsync(
                A<string>.Ignored,
                A<Func<CancellationToken, ValueTask<string>>>.Ignored,
                A<TimeSpan>.Ignored,
                A<IEnumerable<string>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult(cachedValue));

        return cache;
    }

    private static HttpResponseMessage Ok(string content = "") =>
        new(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

    private static HttpResponseMessage NotFound() =>
        new(HttpStatusCode.NotFound);

    private const string ValidIsbn = "9780321146533";
    private const string ValidTitle = "Test Driven Development";
    private const string OpenLibraryCoverUrl =
        "https://covers.openlibrary.org/b/isbn/9780321146533-L.jpg?default=false";

    private const string GoogleBooksJson = """
        {
          "items": [
            {
              "volumeInfo": {
                "imageLinks": {
                  "thumbnail": "http://books.google.com/books/content?id=XXX&printsec=frontcover"
                }
              }
            }
          ]
        }
        """;

    // ─────────────────────────────────────────────────────────────
    //  BuildCacheKey — unit tests (internal method, tested directly)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void BuildCacheKey_SameInputDifferentCase_ReturnsSameKey()
    {
        var key1 = BookCoverRetrievalService.BuildCacheKey("9780321146533", "Test Driven Development");
        var key2 = BookCoverRetrievalService.BuildCacheKey("9780321146533", "TEST DRIVEN DEVELOPMENT");

        key1.Should().Be(key2);
    }

    [Fact]
    public void BuildCacheKey_DifferentIsbnSameTitle_ReturnsDifferentKey()
    {
        var key1 = BookCoverRetrievalService.BuildCacheKey("9780321146533", "Clean Code");
        var key2 = BookCoverRetrievalService.BuildCacheKey("9780132350884", "Clean Code");

        key1.Should().NotBe(key2);
    }

    [Fact]
    public void BuildCacheKey_SameIsbnDifferentTitle_ReturnsDifferentKey()
    {
        var key1 = BookCoverRetrievalService.BuildCacheKey("9780321146533", "Title A");
        var key2 = BookCoverRetrievalService.BuildCacheKey("9780321146533", "Title B");

        key1.Should().NotBe(key2);
    }

    [Fact]
    public void BuildCacheKey_SeparatorInjectionAttempt_DoesNotCollide()
    {
        // Without hashing "isbn|foo|title|bar" vs "isbn|foo|title|" + "bar" could collide.
        var key1 = BookCoverRetrievalService.BuildCacheKey("foo|title|bar", "baz");
        var key2 = BookCoverRetrievalService.BuildCacheKey("foo", "bar|baz");

        key1.Should().NotBe(key2);
    }

    [Fact]
    public void BuildCacheKey_AlwaysProduces64HexCharsAfterPrefix()
    {
        var key = BookCoverRetrievalService.BuildCacheKey(ValidIsbn, ValidTitle);

        // "book-cover:" (11 chars) + 64-char hex
        key.Should().StartWith("book-cover:");
        key["book-cover:".Length..].Should().HaveLength(64)
            .And.MatchRegex("^[0-9a-f]+$");
    }

    // ─────────────────────────────────────────────────────────────
    //  Happy Paths
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCoverUrlAsync_OpenLibraryReturns200_ReturnsOpenLibraryUrl()
    {
        var httpClient = CreateHttpClient(Ok());
        var service = new BookCoverRetrievalService(httpClient, CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be(OpenLibraryCoverUrl);
    }

    [Fact]
    public async Task GetCoverUrlAsync_OpenLibraryReturns404_FallsBackToGoogleBooks_ReturnsHttpsUrl()
    {
        // Open Library returns 404 for EVERY request; Google Books returns a valid response.
        int callIndex = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                var response = callIndex++ == 0
                    ? NotFound()          // Open Library
                    : Ok(GoogleBooksJson); // Google Books
                return Task.FromResult(response);
            });

        var httpClient = new HttpClient(handler);
        var service = new BookCoverRetrievalService(httpClient, CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().StartWith("https://");
        result.Should().Contain("books.google.com");
    }

    [Fact]
    public async Task GetCoverUrlAsync_GoogleBooksThumbnail_IsUpgradedToHttps()
    {
        // Open Library 404, Google Books returns an http:// thumbnail.
        int callIndex = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                var response = callIndex++ == 0 ? NotFound() : Ok(GoogleBooksJson);
                return Task.FromResult(response);
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().StartWith("https://",
            because: "Google Books thumbnails returned over http:// must be upgraded to https://");
        result.Should().NotStartWith("http://books");
    }

    [Fact]
    public async Task GetCoverUrlAsync_BothProvidersFail_ReturnsDefaultCover()
    {
        var httpClient = CreateHttpClient(NotFound());
        var service = new BookCoverRetrievalService(httpClient, CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be("DefaultBookCoverImage.png");
    }

    // ─────────────────────────────────────────────────────────────
    //  Caching Behaviour
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCoverUrlAsync_ValueAlreadyCached_DoesNotCallHttpClient()
    {
        const string cachedUrl = "https://cached.example.com/cover.jpg";
        var cache = CreatePreloadedCache(cachedUrl);
        var handler = A.Fake<HttpMessageHandler>();
        var httpClient = new HttpClient(handler);

        var service = new BookCoverRetrievalService(httpClient, cache,
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be(cachedUrl);
        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task GetCoverUrlAsync_ResultIsStoredInCacheWithCorrectTag()
    {
        var cache = A.Fake<IAppCache>();
        string? capturedKey = null;
        IEnumerable<string>? capturedTags = null;

        A.CallTo(() => cache.GetOrCreateAsync(
                A<string>.Ignored,
                A<Func<CancellationToken, ValueTask<string>>>.Ignored,
                A<TimeSpan>.Ignored,
                A<IEnumerable<string>>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string key, Func<CancellationToken, ValueTask<string>> factory,
                TimeSpan _, IEnumerable<string> tags, CancellationToken ct) =>
            {
                capturedKey = key;
                capturedTags = tags;
                return factory(ct).AsTask();
            });

        var service = new BookCoverRetrievalService(
            CreateHttpClient(Ok()),
            cache,
            NullLogger<BookCoverRetrievalService>.Instance);

        await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        capturedKey.Should().StartWith("book-cover:");
        capturedTags.Should().Contain(BookCoverCachingConstants.BookCoverTag);
    }

    [Fact]
    public async Task GetCoverUrlAsync_DifferentIsbn_UsedifferentCacheKeys()
    {
        var capturedKeys = new List<string>();
        var cache = A.Fake<IAppCache>();

        A.CallTo(() => cache.GetOrCreateAsync(
                A<string>.Ignored,
                A<Func<CancellationToken, ValueTask<string>>>.Ignored,
                A<TimeSpan>.Ignored,
                A<IEnumerable<string>>.Ignored,
                A<CancellationToken>.Ignored))
            .ReturnsLazily((string key, Func<CancellationToken, ValueTask<string>> factory,
                TimeSpan _, IEnumerable<string> __, CancellationToken ct) =>
            {
                capturedKeys.Add(key);
                return factory(ct).AsTask();
            });

        var service = new BookCoverRetrievalService(
            CreateHttpClient(NotFound()),
            cache,
            NullLogger<BookCoverRetrievalService>.Instance);

        await service.GetCoverUrlAsync("9780321146533", ValidTitle);
        await service.GetCoverUrlAsync("9780132350884", ValidTitle);

        capturedKeys.Should().HaveCount(2);
        capturedKeys[0].Should().NotBe(capturedKeys[1]);
    }

    // ─────────────────────────────────────────────────────────────
    //  Edge Cases — OpenLibrary
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCoverUrlAsync_NullIsbn_SkipsOpenLibraryAndTriesGoogleBooks()
    {
        int callCount = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                callCount++;
                return Task.FromResult(Ok(GoogleBooksJson));
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(string.Empty, ValidTitle);

        // Only one HTTP call should happen (Google Books); Open Library is skipped for empty ISBN.
        callCount.Should().Be(1);
        result.Should().Contain("books.google.com");
    }

    [Fact]
    public async Task GetCoverUrlAsync_IsbnWithHyphens_StripsHyphensInUrl()
    {
        string? requestedUrl = null;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(call =>
            {
                var request = call.Arguments.Get<HttpRequestMessage>(0)!;
                requestedUrl = request.RequestUri?.ToString();
                return Task.FromResult(Ok());
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        await service.GetCoverUrlAsync("978-0-321-14653-3", ValidTitle);

        requestedUrl.Should().Contain("9780321146533",
            because: "hyphens must be stripped before building the Open Library URL");
    }

    // ─────────────────────────────────────────────────────────────
    //  Edge Cases — Google Books
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCoverUrlAsync_GoogleBooksReturnsEmptyItems_ReturnsDefaultCover()
    {
        const string emptyItemsJson = """{"items": []}""";
        int callIndex = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                var response = callIndex++ == 0 ? NotFound() : Ok(emptyItemsJson);
                return Task.FromResult(response);
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be("DefaultBookCoverImage.png");
    }

    [Fact]
    public async Task GetCoverUrlAsync_GoogleBooksReturnsNoImageLinks_ReturnsDefaultCover()
    {
        const string noImageLinksJson = """
            {"items": [{"volumeInfo": {"title": "A Book Without Cover"}}]}
            """;
        int callIndex = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                var response = callIndex++ == 0 ? NotFound() : Ok(noImageLinksJson);
                return Task.FromResult(response);
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be("DefaultBookCoverImage.png");
    }

    [Fact]
    public async Task GetCoverUrlAsync_GoogleBooksReturnsMalformedJson_ReturnsDefaultCover()
    {
        int callIndex = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                var response = callIndex++ == 0
                    ? NotFound()
                    : Ok("{this is not valid json{{{{");
                return Task.FromResult(response);
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        // Must not throw — malformed JSON is caught and logged as a warning.
        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be("DefaultBookCoverImage.png");
    }

    [Fact]
    public async Task GetCoverUrlAsync_EmptyTitle_SkipsGoogleBooksAndReturnsDefaultCover()
    {
        var httpClient = CreateHttpClient(NotFound()); // OL always 404
        var service = new BookCoverRetrievalService(httpClient, CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        // Empty title → Google Books is skipped entirely.
        var result = await service.GetCoverUrlAsync(ValidIsbn, string.Empty);

        result.Should().Be("DefaultBookCoverImage.png");
    }

    // ─────────────────────────────────────────────────────────────
    //  Resilience: HttpRequestException
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCoverUrlAsync_OpenLibraryThrowsHttpRequestException_FallsBackToGoogleBooks()
    {
        int callIndex = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                if (callIndex++ == 0)
                    throw new HttpRequestException("Simulated network error");
                return Task.FromResult(Ok(GoogleBooksJson));
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Contain("books.google.com");
    }

    [Fact]
    public async Task GetCoverUrlAsync_BothProvidersThrowHttpRequestException_ReturnsDefaultCover()
    {
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .Throws(new HttpRequestException("Simulated network error"));

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be("DefaultBookCoverImage.png");
    }

    // ─────────────────────────────────────────────────────────────
    //  Resilience: Cancellation
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCoverUrlAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // The cache passes through to the factory.
        var cache = CreatePassThroughCache();

        // The handler throws TaskCanceledException when the token is already cancelled.
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .Throws<TaskCanceledException>();

        var service = new BookCoverRetrievalService(new HttpClient(handler), cache,
            NullLogger<BookCoverRetrievalService>.Instance);

        // OperationCanceledException (base of TaskCanceledException) must propagate —
        // it must NOT be silently swallowed and converted to "default-cover.png".
        Func<Task> act = () => service.GetCoverUrlAsync(ValidIsbn, ValidTitle, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
    // ─────────────────────────────────────────────────────────────
    //  Resilience: Per-call Timeouts
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCoverUrlAsync_OpenLibraryTimesOut_FallsBackToGoogleBooks()
    {
        // First call (Open Library) blocks until TaskCanceledException is thrown,
        // simulating a per-call timeout.  Second call (Google Books) succeeds.
        int callIndex = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                if (callIndex++ == 0)
                    // Simulate the per-call CTS firing (OperationCanceledException subtype).
                    throw new TaskCanceledException("Open Library per-call timeout");

                return Task.FromResult(Ok(GoogleBooksJson));
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Contain("books.google.com");
    }

    [Fact]
    public async Task GetCoverUrlAsync_BothProvidersTimeout_ReturnsDefaultCover()
    {
        // Both providers throw TaskCanceledException (per-call timeout simulation).
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .Throws<TaskCanceledException>();

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be("DefaultBookCoverImage.png");
    }

    // ─────────────────────────────────────────────────────────────
    //  Edge Cases — Google Books (null thumbnail)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCoverUrlAsync_GoogleBooksReturnsNullThumbnailString_ReturnsDefaultCover()
    {
        // The JSON `thumbnail` property is present but has a JSON null value.
        const string nullThumbnailJson = """
            {
              "items": [
                {
                  "volumeInfo": {
                    "imageLinks": {
                      "thumbnail": null
                    }
                  }
                }
              ]
            }
            """;

        int callIndex = 0;
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(() =>
            {
                var response = callIndex++ == 0 ? NotFound() : Ok(nullThumbnailJson);
                return Task.FromResult(response);
            });

        var service = new BookCoverRetrievalService(new HttpClient(handler), CreatePassThroughCache(),
            NullLogger<BookCoverRetrievalService>.Instance);

        var result = await service.GetCoverUrlAsync(ValidIsbn, ValidTitle);

        result.Should().Be("default-cover.png",
            because: "a JSON-null thumbnail must be treated the same as a missing thumbnail");
    }

    // ─────────────────────────────────────────────────────────────
    //  Cache key — driven by BookCoverCachingConstants
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void BuildCacheKey_PrefixMatchesConstant()
    {
        // The cache key prefix must come from the shared constant, not a
        // magic string buried inside the service.
        var key = BookCoverRetrievalService.BuildCacheKey(ValidIsbn, ValidTitle);

        key.Should().StartWith(BookCoverCachingConstants.CacheKeyPrefix,
            because: "the key prefix must be driven by BookCoverCachingConstants.CacheKeyPrefix");
    }
}
