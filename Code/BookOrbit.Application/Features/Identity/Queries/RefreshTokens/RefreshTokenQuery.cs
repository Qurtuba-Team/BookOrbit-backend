namespace BookOrbit.Application.Features.Identity.Queries.RefreshTokens;
public record RefreshTokenQuery(
    string RefreshToken,string ExpiredAccessToken) : IRequest<Result<TokenDto>>;