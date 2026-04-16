namespace BookOrbit.Application.Features.Identity.Queries.GenerateTokens;

public class GenerateTokenQueryHandler(
    ILogger<GenerateTokenQueryHandler>logger,
    ITokenProvider tokenProvider,
    IIdentityService identityService)
    : IRequestHandler<GenerateTokenQuery, Result<TokenDto>>
{
    public async Task<Result<TokenDto>> Handle(GenerateTokenQuery query, CancellationToken ct)
    {
        var authenticationResult = await identityService.AuthenticateAsync(query.Email, query.Password,ct);

        if (authenticationResult.IsFailure)
            return authenticationResult.Errors;

        var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(authenticationResult.Value, ct);

        if(generateTokenResult.IsFailure)
        {
            logger.LogError("Generate token error occurred: {ErrorDescription}", generateTokenResult.TopError.Description);

            return generateTokenResult.Errors;
        }

        return generateTokenResult.Value;
    }
}