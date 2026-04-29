
namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsLostBorrowingTransaction;
public class MarkAsLostBorrowingTransactionCommandValidator : AbstractValidator<MarkAsLostBorrowingTransactionCommand>
{
    public MarkAsLostBorrowingTransactionCommandValidator()
    {
        RuleFor(x => x.BorrowingTransactionId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionIdRules();
    }
}
