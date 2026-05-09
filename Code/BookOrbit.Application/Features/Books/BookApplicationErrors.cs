namespace BookOrbit.Application.Features.Books;
public static class BookApplicationErrors
{
    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(BookErrors.ClassName, "Id", "Id");
    static public readonly Error IsbnAlreadyExists = ApplicationCommonErrors.CustomConflict(BookErrors.ClassName, "IsbnAlreadyExists", "There is a book with the same Isbn");
    static public readonly Error BookIsNotAvailable = ApplicationCommonErrors.CustomConflict(BookErrors.ClassName, "BookIsNotAvailable", "The book is not available for this operation");
    static public readonly Error IsUsedByBookCopies = ApplicationCommonErrors.CustomConflict(BookErrors.ClassName, "IsUsedByBookCopies", "The book is used by book copies and cannot be deleted");
}