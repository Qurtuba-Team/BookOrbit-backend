namespace BookOrbit.Application.Features.Students.Queries.GetStudentPersonalPhotoFileNameById;

public class GetStudentPersonalPhotoFileNameByIdQueryValidator : AbstractValidator<GetStudentPersonalPhotoFileNameByIdQuery>
{
    public GetStudentPersonalPhotoFileNameByIdQueryValidator()
    {
        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}