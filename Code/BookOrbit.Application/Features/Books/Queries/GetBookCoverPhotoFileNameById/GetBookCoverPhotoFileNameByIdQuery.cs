namespace BookOrbit.Application.Features.Books.Queries.GetBookCoverPhotoFileNameById;
public record GetBookCoverPhotoFileNameByIdQuery(
    Guid BookId) : IRequest<Result<string>>;