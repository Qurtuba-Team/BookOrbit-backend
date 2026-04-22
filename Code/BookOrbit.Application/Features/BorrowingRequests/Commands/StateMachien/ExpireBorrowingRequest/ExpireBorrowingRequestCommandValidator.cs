namespace BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.ExpireBorrowingRequest;
public class ExpireBorrowingRequestCommandValidator : AbstractValidator<ExpireBorrowingRequestCommand>
{
    public ExpireBorrowingRequestCommandValidator()
    {
        RuleFor(x => x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();
    }
}
