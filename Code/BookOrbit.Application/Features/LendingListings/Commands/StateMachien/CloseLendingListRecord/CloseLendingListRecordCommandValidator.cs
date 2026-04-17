namespace BookOrbit.Application.Features.LendingListings.Commands.StateMachien.CloseLendingListRecord;
public class CloseLendingListRecordCommandValidator : AbstractValidator<CloseLendingListRecordCommand>
{
    public CloseLendingListRecordCommandValidator()
    {
        RuleFor(x => x.LendingListRecordId)
            .Cascade(CascadeMode.Stop)
            .LendingListIdRules();
    }
}
