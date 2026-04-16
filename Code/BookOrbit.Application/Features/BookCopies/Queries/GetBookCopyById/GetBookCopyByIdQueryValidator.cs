namespace BookOrbit.Application.Features.BookCopies.Queries.GetBookCopyById;
public class GetBookCopyByIdQueryValidator : AbstractValidator<GetBookCopyByIdQuery>
{
    public GetBookCopyByIdQueryValidator()
    {
        RuleFor(x => x.BookCopyId)
            .Cascade(CascadeMode.Stop)
            .BookCopyIdRules();
    }
}