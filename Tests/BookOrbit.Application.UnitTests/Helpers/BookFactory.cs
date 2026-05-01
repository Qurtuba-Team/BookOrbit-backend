using BookOrbit.Domain.Books.ValueObjects;
using BookOrbit.Domain.Books.Enums;

namespace BookOrbit.Application.UnitTests.Helpers;

/// <summary>Factory helpers for creating valid domain objects in tests.</summary>
public static class BookFactory
{
    private static int _isbnCounter = 1000000000;

    public static Book CreateValid(
        Guid? id = null,
        string title = "Test Book",
        BookCategory category = BookCategory.Fiction,
        string author = "Test Author",
        string publisher = "Test Publisher",
        string? isbn = null,
        string coverImageFileName = "cover.jpg")
    {
        var uniqueIsbn = isbn ?? (++_isbnCounter).ToString("D10");

        return Book.Create(
            id ?? Guid.NewGuid(),
            BookTitle.Create(title).Value,
            ISBN.Create(uniqueIsbn).Value,
            BookPublisher.Create(publisher).Value,
            category,
            BookAuthor.Create(author).Value,
            coverImageFileName).Value;
    }
}
