namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;
public class AcceptBorrowingRequestCommandValidator : AbstractValidator<AcceptBorrowingRequestCommand>
{
    public AcceptBorrowingRequestCommandValidator()
    {
        RuleFor(x => x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();

        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}
