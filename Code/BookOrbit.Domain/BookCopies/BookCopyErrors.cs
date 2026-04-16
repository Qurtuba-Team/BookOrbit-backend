namespace BookOrbit.Domain.BookCopies
{
    static public class BookCopyErrors
    {
        private const string ClassName = nameof(BookCopy);

        static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
        static public readonly Error OwnerIdRequired = DomainCommonErrors.RequiredProp(ClassName, "OwnerId", "Owner Id");
        static public readonly Error BookIdRequired = DomainCommonErrors.RequiredProp(ClassName, "BookId", "Book Id");
        static public readonly Error InvalidCondition = DomainCommonErrors.InvalidProp(ClassName, "BookCopyCondition", "Book Copy Condition");
    }
}