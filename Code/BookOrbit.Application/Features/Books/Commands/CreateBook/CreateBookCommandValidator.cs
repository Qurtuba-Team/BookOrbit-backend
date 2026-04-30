namespace BookOrbit.Application.Features.Books.Commands.CreateBook;
public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .BookTitleRules();

        RuleFor(x => x.ISBN)
            .Cascade(CascadeMode.Stop)
            .BookISBNRules();

        RuleFor(x => x.Publisher)
            .Cascade(CascadeMode.Stop)
            .BookPublisherRules();

        RuleFor(x => x.Category)
            .Cascade(CascadeMode.Stop)
            .BookCategoryRules();

        RuleFor(x => x.Author)
            .Cascade(CascadeMode.Stop)
            .BookAuthorRules();

    }
}