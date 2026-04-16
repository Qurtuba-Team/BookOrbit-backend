namespace BookOrbit.Application.Features.Identity.Queries.GenerateTokens;
public record GenerateTokenQuery(
    string Email,
    string Password) : IRequest<Result<TokenDto>>;