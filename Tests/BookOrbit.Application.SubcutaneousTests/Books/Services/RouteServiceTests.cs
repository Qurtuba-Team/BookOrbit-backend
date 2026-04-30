namespace BookOrbit.Application.SubcutaneousTests.Books.Services;

using BookOrbit.Infrastructure.Services;
using BookOrbit.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;

/// <summary>
/// Unit tests for <see cref="RouteService"/> focusing on the URL resolution logic
/// that is critical for the book cover retrieval feature.
/// </summary>
public class RouteServiceTests
{
    // ─────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────

    private static RouteService CreateService(string baseUrl = "https://api.example.com")
    {
        var urls = new Urls { BaseUrl = baseUrl };
        var snapshot = new FakeOptionsSnapshot(urls);
        return new RouteService(snapshot);
    }

    /// <summary>Minimal IOptionsSnapshot stub — avoids needing a full DI container.</summary>
    private sealed class FakeOptionsSnapshot(Urls value) : IOptionsSnapshot<Urls>
    {
        public Urls Value { get; } = value;
        public Urls Get(string? name) => Value;
    }

    // ─────────────────────────────────────────────────────────────
    //  GetBookCoverImageRoute() — base URL
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetBookCoverImageRoute_NoArgs_ReturnsBaseUrlWithUploadsPath()
    {
        var service = CreateService("https://api.example.com");

        var result = service.GetBookCoverImageRoute();

        result.Should().Be("https://api.example.com/uploads/books");
    }

    // ─────────────────────────────────────────────────────────────
    //  GetBookCoverImageRoute(string fileName) — local file name
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetBookCoverImageRoute_LocalFileName_ReturnsFullLocalUrl()
    {
        var service = CreateService("https://api.example.com");

        var result = service.GetBookCoverImageRoute("cover.png");

        result.Should().Be("https://api.example.com/uploads/books/cover.png");
    }

    [Fact]
    public void GetBookCoverImageRoute_DefaultFallbackFileName_ReturnsFullLocalUrl()
    {
        var service = CreateService("https://api.example.com");

        var result = service.GetBookCoverImageRoute("default-cover.png");

        result.Should().Be("https://api.example.com/uploads/books/default-cover.png");
    }

    // ─────────────────────────────────────────────────────────────
    //  GetBookCoverImageRoute(string fileName) — absolute URL pass-through
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetBookCoverImageRoute_AbsoluteHttpsUrl_ReturnsUrlUnchanged()
    {
        const string externalUrl = "https://covers.openlibrary.org/b/isbn/9780321146533-L.jpg?default=false";
        var service = CreateService("https://api.example.com");

        var result = service.GetBookCoverImageRoute(externalUrl);

        result.Should().Be(externalUrl, because: "absolute URLs from external providers must be returned as-is");
    }

    [Fact]
    public void GetBookCoverImageRoute_GoogleBooksThumbnailUrl_ReturnsUrlUnchanged()
    {
        const string googleUrl = "https://books.google.com/books/content?id=XXX";
        var service = CreateService("https://api.example.com");

        var result = service.GetBookCoverImageRoute(googleUrl);

        result.Should().Be(googleUrl);
    }

    [Fact]
    public void GetBookCoverImageRoute_AbsoluteHttpUrl_ReturnsUrlUnchanged()
    {
        // We do not mutate the URL in RouteService — that is the retrieval service's concern.
        const string httpUrl = "http://example.com/cover.jpg";
        var service = CreateService("https://api.example.com");

        var result = service.GetBookCoverImageRoute(httpUrl);

        result.Should().Be(httpUrl);
    }

    [Fact]
    public void GetBookCoverImageRoute_RelativeUrl_IsNotTreatedAsAbsolute()
    {
        // A relative path like "/covers/foo.jpg" must be appended to the base, not returned as-is.
        const string relativeUrl = "/covers/foo.jpg";
        var service = CreateService("https://api.example.com");

        var result = service.GetBookCoverImageRoute(relativeUrl);

        // Uri.IsWellFormedUriString("/covers/foo.jpg", Absolute) returns false,
        // so it should be appended.
        result.Should().StartWith("https://api.example.com");
    }

    [Fact]
    public void GetBookCoverImageRoute_UrlWithQueryAndFragment_ReturnsUrlUnchanged()
    {
        const string url = "https://covers.openlibrary.org/b/isbn/0306406152-L.jpg?default=false";
        var service = CreateService("https://api.example.com");

        var result = service.GetBookCoverImageRoute(url);

        result.Should().Be(url);
        result.Should().Contain("?default=false");
    }
}
