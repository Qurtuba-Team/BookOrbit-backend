namespace BookOrbit.Application.Features.Books.Dtos;
public record BookListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public BookCategory Category { get; set; }
    public string Author { get; set; } = string.Empty;
    public int AvailableCopiesCount { get; set; }
    public string BookCoverImageUrl { get; set; } = string.Empty;

    [JsonConstructor]
    private BookListItemDto() { }

    public BookListItemDto(
        Guid id,
        string title,
        string iSBN,
        string publisher,
        BookCategory category,
        string author,
        int availableCopiesCount,
        string bookCoverImageUrl)
    {
        Id = id;
        Title = title;
        ISBN = iSBN;
        Publisher = publisher;
        Category = category;
        Author = author;
        AvailableCopiesCount = availableCopiesCount;
        BookCoverImageUrl = bookCoverImageUrl;
    }
}