
namespace BookOrbit.Application.Features.BookCopies.Commands.CreateBookCopy;
public class CreateBookCopyCommandValidator : AbstractValidator<CreateBookCopyCommand>
{
    public CreateBookCopyCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .Cascade(CascadeMode.Stop)
            .StudentIdRules();

        RuleFor(x => x.BookId)
            .Cascade(CascadeMode.Stop)
            .BookIdRules();

        RuleFor(x => x.Condition)
            .Cascade(CascadeMode.Stop)
            .BookCopyConditionRules();
    }
}
