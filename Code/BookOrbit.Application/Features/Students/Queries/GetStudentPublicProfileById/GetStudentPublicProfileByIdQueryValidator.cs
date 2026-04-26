namespace BookOrbit.Application.Features.Students.Queries.GetStudentPublicProfileById;

public class GetStudentPublicProfileByIdQueryValidator : AbstractValidator<GetStudentPublicProfileByIdQuery>
{
    public GetStudentPublicProfileByIdQueryValidator()
    {
        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}