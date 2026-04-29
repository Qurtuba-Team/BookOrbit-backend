
namespace BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsReturnedBorrowingTransaction;
public class MarkAsReturnedBorrowingTransactionCommandValidator : AbstractValidator<MarkAsReturnedBorrowingTransactionCommand>
{
    public MarkAsReturnedBorrowingTransactionCommandValidator()
    {
        RuleFor(x => x.BorrowingTransactionId)
            .Cascade(CascadeMode.Stop)
            .BorrowingTransactionIdRules();
    }
}
