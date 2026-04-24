namespace BookOrbit.Application.Features.Books.Commands.StateMachien.RejectBook;
public class RejectBookCommandValidator : AbstractValidator<RejectBookCommand>
{
    public RejectBookCommandValidator()
    {
        RuleFor(x => x.BookId)
            .Cascade(CascadeMode.Stop)
            .BookIdRules();
    }
}
