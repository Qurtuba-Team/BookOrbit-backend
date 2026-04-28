using BookOrbit.Application.Features.BorrowingTransactionEvents;

namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEventById;

public class GetBorrowingTransactionEventByIdQueryValidator : AbstractValidator<GetBorrowingTransactionEventByIdQuery>
{
    public GetBorrowingTransactionEventByIdQueryValidator()
    {
        RuleFor(x => x.BorrowingTransactionEventId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionEventIdRules();
    }
}
