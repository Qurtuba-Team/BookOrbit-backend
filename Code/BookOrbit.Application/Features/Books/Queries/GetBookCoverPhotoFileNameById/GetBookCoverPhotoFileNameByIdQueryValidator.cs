namespace BookOrbit.Application.Features.Books.Queries.GetBookCoverPhotoFileNameById;
public class GetBookCoverPhotoFileNameByIdQueryValidator : AbstractValidator<GetBookCoverPhotoFileNameByIdQuery>
{
    public GetBookCoverPhotoFileNameByIdQueryValidator()
    {
        RuleFor(x => x.BookId)
            .Cascade(CascadeMode.Stop)
            .BookIdRules();
    }
}
