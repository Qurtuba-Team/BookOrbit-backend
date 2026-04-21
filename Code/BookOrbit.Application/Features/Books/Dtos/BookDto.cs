namespace BookOrbit.Application.Features.Books.Dtos;
public record BookDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public BookCategory Category { get; set; }
    public string Author { get; set; } = string.Empty;
    public string BookCoverImageUrl { get; set; } = string.Empty;

    [JsonConstructor]
    private BookDto() { }

    private BookDto(
        Guid id,
        string title,
        string isbn,
        string publisher,
        BookCategory category,
        string author,
        string bookCoverImageUrl)
    {
        Id = id;
        Title = title;
        ISBN = isbn;
        Publisher = publisher;
        Category = category;
        Author = author;
        BookCoverImageUrl = bookCoverImageUrl;
    }

    static public BookDto FromEntity(Book book,string bookCoverImageUrl)
    =>
        new(
            book.Id,
            book.Title.Value,
            book.ISBN.Value,
            book.Publisher.Value,
            book.Category,
            book.Author.Value,
            bookCoverImageUrl);


}