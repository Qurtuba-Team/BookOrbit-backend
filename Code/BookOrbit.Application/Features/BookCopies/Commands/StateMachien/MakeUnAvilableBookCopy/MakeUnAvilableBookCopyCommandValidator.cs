namespace BookOrbit.Application.Features.BookCopies.Commands.StateMachien.MakeUnAvilableBookCopy;
public class MakeUnAvilableBookCopyCommandValidator : AbstractValidator<MakeUnAvilableBookCopyCommand>
{
    public MakeUnAvilableBookCopyCommandValidator()
    {
        RuleFor(x => x.BookCopyId)
            .Cascade(CascadeMode.Stop)
            .BookCopyIdRules();

        RuleFor(x => x.RowVersion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty();
    }
}
