namespace BookOrbit.Application.Features.BookCopies;
public static class BookCopyApplicationErrors
{
    
    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(BookCopyErrors.ClassName, "Id", "Id");
    static public readonly Error BookCopyInUse = ApplicationCommonErrors.CustomConflict(
        BookCopyErrors.ClassName,
        "BookCopyInUse",
        $"The book copy is currently in use and cannot be marked as unavailable."
    );
}
