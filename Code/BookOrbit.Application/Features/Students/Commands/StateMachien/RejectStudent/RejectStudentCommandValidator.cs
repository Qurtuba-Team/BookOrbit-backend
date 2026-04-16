namespace BookOrbit.Application.Features.Students.Commands.StateMachien.RejectStudent;
public class RejectStudentCommandValidator : AbstractValidator<RejectStudentCommand>
{
    public RejectStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}
