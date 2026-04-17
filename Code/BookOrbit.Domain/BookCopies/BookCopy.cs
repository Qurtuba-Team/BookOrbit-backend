namespace BookOrbit.Domain.BookCopies;
public class BookCopy : AuditableEntity
{
    public Guid OwnerId { get; }
    public Guid BookId { get; }
    public BookCopyCondition Condition { get; private set; }
    public BookCopyState State { get; private set; }

    public Student? Owner { get; private set; }
    public Book? Book { get; private set; }

    private BookCopy() { }

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

        if (!Enum.IsDefined(condition))
            return BookCopyErrors.InvalidCondition;

        return new BookCopy(id, ownerId, bookId, condition);
    }

    public Result<Updated> Update(
        BookCopyCondition condition)
    {
        if (!Enum.IsDefined(condition))
            return BookCopyErrors.InvalidCondition;

        Condition = condition;

        return Result.Updated;
    }

    private bool CanTransitionToState(BookCopyState newState)
    {
        return State switch
        {
            BookCopyState.Available => newState is BookCopyState.Reserved or BookCopyState.UnAvilable,
            BookCopyState.Reserved => newState is BookCopyState.Borrowed or BookCopyState.Available,
            BookCopyState.Borrowed => newState is BookCopyState.Available or BookCopyState.Lost or BookCopyState.Damaged,
            BookCopyState.Damaged => newState is BookCopyState.Available or BookCopyState.UnAvilable,
            BookCopyState.Lost => newState is BookCopyState.Available or BookCopyState.UnAvilable,
            BookCopyState.UnAvilable => newState is BookCopyState.Available,
            _ => false
        };
    }

    public Result<Updated> UpdateState(BookCopyState newState)
    {
        if (!CanTransitionToState(newState))
            return BookCopyErrors.InvalidStateTransition(State, newState);

        State = newState;
        return Result.Updated;
    }

    public Result<Updated> MarkAsUnAvilable() =>
   UpdateState(BookCopyState.UnAvilable);

    public Result<Updated> MarkAsAvilable() =>
   UpdateState(BookCopyState.Available);

    public Result<Updated> MarkAsBorrowed() =>
   UpdateState(BookCopyState.Borrowed);

    public Result<Updated> MarkAsReserved() =>
   UpdateState(BookCopyState.Reserved);

    public Result<Updated> MarkAsLost() =>
   UpdateState(BookCopyState.Lost);

    public Result<Updated> MarkAsDamaged() =>
   UpdateState(BookCopyState.Damaged);

}