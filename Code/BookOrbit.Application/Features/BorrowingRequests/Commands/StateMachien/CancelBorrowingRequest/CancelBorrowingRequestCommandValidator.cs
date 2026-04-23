namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.CancelBorrowingRequest;
public class CancelBorrowingRequestCommandValidator : AbstractValidator<CancelBorrowingRequestCommand>
{
    public CancelBorrowingRequestCommandValidator()
    {
        RuleFor(x => x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();

        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}
