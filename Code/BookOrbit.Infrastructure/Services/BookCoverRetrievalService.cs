using System.Security.Cryptography;
using System.Text;
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
        // Use a stable, bounded cache key that is immune to separator-injection.
        // SHA-256 of the canonical form keeps the key at a fixed 64-char hex string.
        string cacheKey = BuildCacheKey(isbn, title);

        return await cache.GetOrCreateAsync(
            key: cacheKey,
            factory: token => ResolveAsync(isbn, title, token),
            expiration: CacheExpiration,
            tags: [BookCoverCachingConstants.BookCoverTag],
            cancellationToken: ct);
    }

    /// <summary>
    /// Builds a collision-resistant, bounded cache key from isbn + title.
    /// Normalises to lowercase to avoid case-variant cache misses.
    /// </summary>
    internal static string BuildCacheKey(string isbn, string title)
    {
        string raw = $"isbn|{isbn.ToLowerInvariant()}|title|{title.ToLowerInvariant()}";
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"book-cover:{Convert.ToHexString(hashBytes).ToLowerInvariant()}";
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
        catch (OperationCanceledException)
        {
            // Never swallow cancellation — propagate so the caller can honour it.
            throw;
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

        // Remove hyphens to normalise ISBN format.
        string cleanIsbn = isbn.Replace("-", string.Empty);

        // ?default=false makes Open Library return 404 when no cover exists,
        // rather than serving a 1×1 pixel placeholder image.
        string url = $"https://covers.openlibrary.org/b/isbn/{cleanIsbn}-L.jpg?default=false";

        try
        {
            // Dispose the response to return the connection to the pool promptly.
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
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
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var contentStream = await response.Content.ReadAsStreamAsync(ct);
            using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: ct);

            if (document.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            {
                var firstItem = items[0];
                if (firstItem.TryGetProperty("volumeInfo", out var volumeInfo) &&
                    volumeInfo.TryGetProperty("imageLinks", out var imageLinks) &&
                    imageLinks.TryGetProperty("thumbnail", out var thumbnail))
                {
                    // Google Books serves http:// thumbnails — upgrade to https:// for security.
                    var thumbnailUrl = thumbnail.GetString();
                    return thumbnailUrl is not null
                        ? thumbnailUrl.Replace("http://", "https://", StringComparison.OrdinalIgnoreCase)
                        : null;
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
