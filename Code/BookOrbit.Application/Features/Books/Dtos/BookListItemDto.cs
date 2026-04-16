namespace BookOrbit.Application.Features.Books.Dtos;
public record BookListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public BookCategory Category { get; set; }
    public string Author { get; set; } = string.Empty;


    [JsonConstructor]
    private BookListItemDto() { }

    private BookListItemDto(
        Guid id,
        string title,
        string iSBN,
        string publisher,
        BookCategory category,
        string author)
    {
        Id = id;
        Title = title;
        ISBN = iSBN;
        Publisher = publisher;
        Category = category;
        Author = author;
    }

    public static Expression<Func<Book, BookListItemDto>> Projection =>
        s => new BookListItemDto(
            s.Id,
            s.Title.Value,
            s.ISBN.Value,
            s.Publisher.Value,
            s.Category,
            s.Author.Value);

}