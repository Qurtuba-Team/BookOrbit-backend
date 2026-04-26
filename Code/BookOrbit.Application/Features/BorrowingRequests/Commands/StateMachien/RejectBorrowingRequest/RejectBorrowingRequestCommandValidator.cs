namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.RejectBorrowingRequest;
public class RejectBorrowingRequestCommandValidator : AbstractValidator<RejectBorrowingRequestCommand>
{
    public RejectBorrowingRequestCommandValidator()
    {
        RuleFor(x => x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();
    }
}
