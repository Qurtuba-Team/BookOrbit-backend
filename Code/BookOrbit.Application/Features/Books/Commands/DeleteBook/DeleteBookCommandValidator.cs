namespace BookOrbit.Application.Features.Books.Commands.DeleteBook;
public class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
{
    public DeleteBookCommandValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .BookIdRules();
    }
}