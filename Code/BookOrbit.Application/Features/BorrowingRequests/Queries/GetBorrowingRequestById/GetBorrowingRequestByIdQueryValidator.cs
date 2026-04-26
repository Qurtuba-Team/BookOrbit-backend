namespace BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequestById;
public class GetBorrowingRequestByIdQueryValidator : AbstractValidator<GetBorrowingRequestByIdQuery>
{
    public GetBorrowingRequestByIdQueryValidator()
    {
        RuleFor(x => x.BorrowingRequestId)
            .Cascade(CascadeMode.Stop)
            .BorrowingRequestIdRules();
    }
}
