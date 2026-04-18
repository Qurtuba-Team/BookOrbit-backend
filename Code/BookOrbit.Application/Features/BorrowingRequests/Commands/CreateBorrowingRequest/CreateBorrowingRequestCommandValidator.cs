namespace BookOrbit.Application.Features.BorrowingRequests.Commands.CreateBorrowingRequest;
public class CreateBorrowingRequestCommandValidator : AbstractValidator<CreateBorrowingRequestCommand>
{
    public CreateBorrowingRequestCommandValidator()
    {
        RuleFor(x => x.BorrowingStudentId)
            .Cascade(CascadeMode.Stop)
            .BorrowingStudentIdRules();

        RuleFor(x => x.LendingRecordId)
            .Cascade(CascadeMode.Stop)
            .LendingRecordIdRules();
    }
}
