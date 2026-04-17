namespace BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecordById;
public class GetLendingListRecordByIdQueryValidator : AbstractValidator<GetLendingListRecordByIdQuery>
{
    public GetLendingListRecordByIdQueryValidator()
    {
        RuleFor(x => x.LendingListRecordId)
            .Cascade(CascadeMode.Stop)
            .LendingListIdRules();
    }
}
