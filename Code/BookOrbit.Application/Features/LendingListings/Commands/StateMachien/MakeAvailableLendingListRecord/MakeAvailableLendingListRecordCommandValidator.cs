namespace BookOrbit.Application.Features.LendingListings.Commands.StateMachien.MakeAvailableLendingListRecord;
public class MakeAvailableLendingListRecordCommandValidator : AbstractValidator<MakeAvailableLendingListRecordCommand>
{
    public MakeAvailableLendingListRecordCommandValidator()
    {
        RuleFor(x => x.LendingListRecordId)
            .Cascade(CascadeMode.Stop)
            .LendingListIdRules();
    }
}
