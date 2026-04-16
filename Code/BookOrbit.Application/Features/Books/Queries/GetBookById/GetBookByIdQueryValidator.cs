namespace BookOrbit.Application.Features.Books.Queries.GetBookById;
public class GetBookByIdQueryValidator : AbstractValidator<GetBookByIdQuery>
{
    public GetBookByIdQueryValidator()
    {
        RuleFor(x => x.BookId)
            .Cascade(CascadeMode.Stop)
            .BookIdRules();
    }
}