namespace BookOrbit.Application.SubcutaneousTests.Books.TestDoubles;

using BookOrbit.Application.Common.Interfaces;

public class FakeBookCoverRetrievalService : IBookCoverRetrievalService
{
    public int CallCount { get; private set; }
    public string ReturnValue { get; set; } = "https://covers.openlibrary.org/b/isbn/9780321146533-L.jpg?default=false";

    public Task<string> GetCoverUrlAsync(string isbn, string title, CancellationToken ct = default)
    {
        CallCount++;
        return Task.FromResult(ReturnValue);
    }
}
