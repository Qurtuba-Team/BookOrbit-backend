using BookOrbit.Application.Features.BorrowingTransactions;
using BookOrbit.Application.Features.Students;

namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsLostBorrowingTransaction;
public class MarkAsLostBorrowingTransactionCommandValidator : AbstractValidator<MarkAsLostBorrowingTransactionCommand>
{
    public MarkAsLostBorrowingTransactionCommandValidator()
    {
        RuleFor(x => x.BorrowingTransactionId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionIdRules();

        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}
