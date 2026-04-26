namespace BookOrbit.Application.Features.Students.Queries.GetStudentContactInformationByLendingListId;

public class GetStudentContactInformationByLendingListIdQueryValidator : AbstractValidator<GetStudentContactInformationByLendingListIdQuery>
{
    public GetStudentContactInformationByLendingListIdQueryValidator()
    {
        RuleFor(x => x.LendingListId)
            .Cascade(CascadeMode.Stop)
            .LendingListIdRules();

        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}
