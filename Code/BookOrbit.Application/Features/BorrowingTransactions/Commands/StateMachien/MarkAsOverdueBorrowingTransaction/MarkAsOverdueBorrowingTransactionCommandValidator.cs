using BookOrbit.Application.Features.BorrowingTransactions;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsOverdueBorrowingTransaction;
public class MarkAsOverdueBorrowingTransactionCommandValidator : AbstractValidator<MarkAsOverdueBorrowingTransactionCommand>
{
    public MarkAsOverdueBorrowingTransactionCommandValidator()
    {
        RuleFor(x => x.BorrowingTransactionId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionIdRules();
    }
}
