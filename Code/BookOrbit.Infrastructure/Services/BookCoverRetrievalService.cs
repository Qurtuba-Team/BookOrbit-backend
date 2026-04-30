using System.Text.Json;
using BookOrbit.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookOrbit.Infrastructure.Services;

public class BookCoverRetrievalService(
    HttpClient httpClient,
    IAppCache cache,
    ILogger<BookCoverRetrievalService> logger) : IBookCoverRetrievalService
{
    private const string DefaultCoverImage = "default-cover.png";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    public async Task<string> GetCoverUrlAsync(string isbn, string title, CancellationToken ct = default)
    {
        string cacheKey = $"book-cover:isbn={isbn}:title={title}";

        return await cache.GetOrCreateAsync(
            key: cacheKey,
            factory: token => ResolveAsync(isbn, title, token),
            expiration: CacheExpiration,
            tags: [BookCoverCachingConstants.BookCoverTag],
            cancellationToken: ct);
    }

    private async ValueTask<string> ResolveAsync(string isbn, string title, CancellationToken ct)
    {
        try
        {
            var openLibraryUrl = await TryGetFromOpenLibraryAsync(isbn, ct);
            if (!string.IsNullOrEmpty(openLibraryUrl))
            {
                logger.LogInformation(
                    "Successfully retrieved book cover from Open Library for ISBN {Isbn}", isbn);
                return openLibraryUrl;
            }

            var googleBooksUrl = await TryGetFromGoogleBooksAsync(title, ct);
            if (!string.IsNullOrEmpty(googleBooksUrl))
            {
                logger.LogInformation(
                    "Successfully retrieved book cover from Google Books for Title {Title}", title);
                return googleBooksUrl;
            }

            logger.LogWarning(
                "Failed to retrieve book cover from both providers for ISBN {Isbn} and Title {Title}. Using default cover.",
                isbn, title);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An unexpected error occurred while retrieving book cover for ISBN {Isbn} and Title {Title}. Using default cover.",
                isbn, title);
        }

        return DefaultCoverImage;
    }

    private async Task<string?> TryGetFromOpenLibraryAsync(string isbn, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return null;

        // Clean ISBN by removing hyphens
        string cleanIsbn = isbn.Replace("-", string.Empty);

        // ?default=false ensures Open Library returns 404 when no cover exists,
        // rather than serving a 1x1 pixel placeholder image.
        string url = $"https://covers.openlibrary.org/b/isbn/{cleanIsbn}-L.jpg?default=false";

        try
        {
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (response.IsSuccessStatusCode)
                return url;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex,
                "HTTP request failed when contacting Open Library for ISBN {Isbn}", isbn);
        }

        return null;
    }

    private async Task<string?> TryGetFromGoogleBooksAsync(string title, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        string query = Uri.EscapeDataString($"intitle:{title}");
        string url = $"https://www.googleapis.com/books/v1/volumes?q={query}";

        try
        {
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (response.IsSuccessStatusCode)
            {
                var contentStream = await response.Content.ReadAsStreamAsync(ct);
                using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: ct);

                if (document.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    var firstItem = items[0];
                    if (firstItem.TryGetProperty("volumeInfo", out var volumeInfo) &&
                        volumeInfo.TryGetProperty("imageLinks", out var imageLinks) &&
                        imageLinks.TryGetProperty("thumbnail", out var thumbnail))
                    {
                        return thumbnail.GetString();
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                "Failed to parse JSON response from Google Books for Title {Title}", title);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex,
                "HTTP request failed when contacting Google Books for Title {Title}", title);
        }

        return null;
    }
}
