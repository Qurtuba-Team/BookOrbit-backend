namespace BookOrbit.Application.Features.BookCopies;
public static class BookCopyApplicationErrors
{
    private const string ClassName = nameof(BookCopy);

    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
}
