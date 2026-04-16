namespace BookOrbit.Application.Features.Students.Queries.GetStudentByUserId;
public class GetStudentByUserIdQueryValidator : AbstractValidator<GetStudentByUserIdQuery>
{
    public GetStudentByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(StudentErrors.UserIdRequired.Description);
    }
}