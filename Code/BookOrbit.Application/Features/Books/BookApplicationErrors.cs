namespace BookOrbit.Application.Features.Books;
public static class BookApplicationErrors
{
    private const string ClassName = nameof(Book);

    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
    static public readonly Error IsbnAlreadyExists = ApplicationCommonErrors.CustomConflict(ClassName, "IsbnAlreadyExists", "There is a book with the same Isbn");
}