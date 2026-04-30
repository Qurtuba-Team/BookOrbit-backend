namespace BookOrbit.Application.Common.Interfaces;

public interface IBookCoverRetrievalService
{
    /// <summary>
    /// Retrieves a valid cover image URL for a given book using its ISBN or Title.
    /// Returns a valid URL or a default fallback image path if not found.
    /// </summary>
    Task<string> GetCoverUrlAsync(string isbn, string title, CancellationToken ct = default);
}
