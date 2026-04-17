namespace BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeAvilableBookCopy;
public class MakeAvilableBookCopyCommandValidator : AbstractValidator<MakeAvilableBookCopyCommand>
{
    public MakeAvilableBookCopyCommandValidator()
    {
        RuleFor(x => x.BookCopyId)
            .Cascade(CascadeMode.Stop)
            .BookCopyIdRules();
    }
}
