using BookOrbit.Application.Features.PointTransactions;

namespace BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactionById;

public class GetPointTransactionByIdQueryValidator : AbstractValidator<GetPointTransactionByIdQuery>
{
    public GetPointTransactionByIdQueryValidator()
    {
        RuleFor(x => x.PointTransactionId)
            .Cascade(CascadeMode.Stop)
            .PointTransactionIdRules();
    }
}
