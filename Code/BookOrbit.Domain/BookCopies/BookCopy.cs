namespace BookOrbit.Domain.BookCopies;
public class BookCopy : AuditableEntity
{
    public Guid OwnerId { get; }
    public Guid BookId { get; }
    public BookCopyCondition Condition { get; private set;}
    public BookCopyState State { get; }

    public Student? Owner { get; private set; }
    public Book? Book { get; private set; }
    
    private BookCopy(){ }

    private BookCopy(
        Guid id,
        Guid ownerId,
        Guid bookId,
        BookCopyCondition condition) : base(id)
    {
        OwnerId = ownerId;
        BookId = bookId;
        Condition = condition;
        State = BookCopyState.Available;
    }

    public static Result<BookCopy> Create(
        Guid id,
        Guid ownerId,
        Guid bookId,
        BookCopyCondition condition)
    {
        if (id == Guid.Empty)
            return BookCopyErrors.IdRequired;

        if (ownerId == Guid.Empty)
            return BookCopyErrors.OwnerIdRequired;

        if (bookId == Guid.Empty)
            return BookCopyErrors.BookIdRequired;

        if(!Enum.IsDefined(condition))
            return BookCopyErrors.InvalidCondition;

        return new BookCopy(id, ownerId, bookId, condition);
    }

    public Result<Updated> Update(
        BookCopyCondition condition)
    {
        if(!Enum.IsDefined(condition))
            return BookCopyErrors.InvalidCondition;

        Condition = condition;

        return Result.Updated;
    }
}