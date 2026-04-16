namespace BookOrbit.Application.Features.Students.Commands.StateMachien.UnBanStudent;
public class UnBanStudentCommandValidator : AbstractValidator<UnBanStudentCommand>
{
    public UnBanStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}
