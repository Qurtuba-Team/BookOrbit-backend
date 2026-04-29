namespace BookOrbit.Application.Features.LendingListings.Commands.CreateLendingListRecord;
public class CreateLendingListRecordCommandValidator : AbstractValidator<CreateLendingListRecordCommand>
{
    public CreateLendingListRecordCommandValidator()
    {
        RuleFor(x=>x.BookCopyId)
            .Cascade(CascadeMode.Stop)
            .BookCopyIdRules();

        RuleFor(x => x.BorrowingDurationInDays)
            .Cascade(CascadeMode.Stop)
            .BorrowingDurationInDaysRules();
    }
}