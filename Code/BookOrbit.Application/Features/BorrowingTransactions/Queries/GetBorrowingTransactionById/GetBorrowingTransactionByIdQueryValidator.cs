using BookOrbit.Application.Features.BorrowingTransactions;

namespace BookOrbit.Application.Features.BorrowingTransactions.Queries.GetBorrowingTransactionById;
public class GetBorrowingTransactionByIdQueryValidator : AbstractValidator<GetBorrowingTransactionByIdQuery>
{
    public GetBorrowingTransactionByIdQueryValidator()
    {
        RuleFor(x => x.BorrowingTransactionId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionIdRules();
    }
}
