namespace BookOrbit.Application.Features.Books.Commands.StateMachien.MakeBookAvilable;
public class MakeBookAvilableCommandValidator : AbstractValidator<MakeBookAvilableCommand>
{
    public MakeBookAvilableCommandValidator()
    {
        RuleFor(x => x.BookId)
            .Cascade(CascadeMode.Stop)
            .BookIdRules();
    }
}
