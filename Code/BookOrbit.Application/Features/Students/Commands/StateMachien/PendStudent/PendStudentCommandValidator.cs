namespace BookOrbit.Application.Features.Students.Commands.StateMachien.PendStudent;
public class PendStudentCommandValidator : AbstractValidator<PendStudentCommand>
{
    public PendStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();
    }
}