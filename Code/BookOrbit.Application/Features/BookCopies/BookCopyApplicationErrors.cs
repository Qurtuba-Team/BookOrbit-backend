namespace BookOrbit.Application.Features.BookCopies;
public static class BookCopyApplicationErrors
{
    private const string ClassName = nameof(BookCopy);

    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
    static public readonly Error BookCopyInUse = ApplicationCommonErrors.CustomConflict(
        ClassName,
        "BookCopyInUse",
        $"The book copy is currently in use and cannot be marked as unavailable."
    );
}
