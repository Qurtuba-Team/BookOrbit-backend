namespace BookOrbit.Application.Features.BookCopies.Commands.UpdateBookCopy;
public class UpdateBookCopyCommandValidator : AbstractValidator<UpdateBookCopyCommand>
{
    public UpdateBookCopyCommandValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .BookCopyIdRules();

        RuleFor(x => x.Condition)
            .Cascade(CascadeMode.Stop)
            .BookCopyConditionRules();
    }
}