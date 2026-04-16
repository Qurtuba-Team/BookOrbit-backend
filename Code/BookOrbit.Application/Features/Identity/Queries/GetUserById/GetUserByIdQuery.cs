namespace BookOrbit.Application.Features.Identity.Queries.GetUserById;
public record GetUserByIdQuery(
    string? UserId) : IRequest<Result<AppUserDto>>;