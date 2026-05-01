using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Features.Books.Queries.GetBooks;
using BookOrbit.Application.Features.Students.Queries.GetStudentByUserId;
using MediatR;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace BookOrbit.Infrastructure.Services.Chatbot;

/// <summary>
/// Kernel plugin that exposes BookOrbit platform data to the LLM via function calling.
/// Each method dispatches to an existing MediatR query — no new business logic here.
/// </summary>
public class BookOrbitKernelPlugin(ISender sender, ICurrentUser currentUser)
{
    [KernelFunction("search_books")]
    [Description("Searches the BookOrbit catalog for books. Call this when the user asks to find or look up books.")]
    public async Task<string> SearchBooksAsync(
        [Description("A search term to filter books by title, author, or publisher. Can be empty to list all books.")] string searchTerm,
        [Description("Optional book category to filter by (e.g. Science, Literature, Technology). Omit if not specified by the user.")] string? category = null,
        CancellationToken cancellationToken = default)
    {
        List<BookCategory>? categories = null;
        if (!string.IsNullOrWhiteSpace(category) &&
            Enum.TryParse<BookCategory>(category, ignoreCase: true, out var parsedCategory))
        {
            categories = [parsedCategory];
        }

        var query = new GetBooksQuery(
            Page: 1,
            PageSize: 5,
            SearchTerm: string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
            Categories: categories,
            Statuses: [BookStatus.Available]);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return "No books were found matching your search.";

        var books = result.Value!;
        var items = books.Items;

        if (items is null || items.Count == 0)
            return "No books were found matching your search.";

        var lines = items
            .Select(b => $"• \"{b.Title}\" by {b.Author} (ISBN: {b.ISBN}) — {b.Category}");

        return $"Found {books.TotalCount} book(s). Showing first {items.Count}:\n" +
               string.Join("\n", lines);
    }

    [KernelFunction("get_my_points")]
    [Description("Returns the current student's points balance. Call this when the user asks how many points they have.")]
    public async Task<string> GetMyPointsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.Id;
        if (string.IsNullOrWhiteSpace(userId))
            return "I could not identify your account. Please log in and try again.";

        var result = await sender.Send(new GetStudentByUserIdQuery(userId), cancellationToken);

        if (result.IsFailure)
            return "I could not retrieve your points balance at this time.";

        return $"You currently have {result.Value.Points} point(s).";
    }

    [KernelFunction("get_my_profile")]
    [Description("Returns the current student's profile information such as name and status. Call this when the user asks about their account or profile.")]
    public async Task<string> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.Id;
        if (string.IsNullOrWhiteSpace(userId))
            return "I could not identify your account. Please log in and try again.";

        var result = await sender.Send(new GetStudentByUserIdQuery(userId), cancellationToken);

        if (result.IsFailure)
            return "I could not retrieve your profile at this time.";

        var s = result.Value;
        return $"Name: {s.Name} | Status: {s.State} | Points: {s.Points}";
    }
}
