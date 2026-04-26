using BookOrbit.Application.Features.BorrowingTransactions;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.CreateBorrowingTransaction;
public class CreateBorrowingTransactionCommandValidator : AbstractValidator<CreateBorrowingTransactionCommand>
{
    public CreateBorrowingTransactionCommandValidator()
    {
        RuleFor(x => x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();
    }
}
