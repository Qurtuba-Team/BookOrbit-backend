namespace BookOrbit.Application.Features.Identity.Queries.RefreshTokens;
public class RefreshTokenQueryHandler(
    ILogger<RefreshTokenQueryHandler> logger,
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    IAppDbContext context) : IRequestHandler<RefreshTokenQuery, Result<TokenDto>>
{
    public async Task<Result<TokenDto>> Handle(RefreshTokenQuery query, CancellationToken ct)
    {
        var principal = tokenProvider.GetPrincipalFromExpiredToken(query.ExpiredAccessToken);

        if (principal is null)
        {
            logger.LogWarning("Expired access token is not valid");

            return IdentityApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            logger.LogWarning("Invalid userId claim");

            return IdentityApplicationErrors.UserIdClaimInvalid;
        }

        var getUserResult = await identityService.GetUserByIdAsync(userId, ct);

        if (getUserResult.IsFailure)
        {
            logger.LogError("Get user by id error occurred: {ErrorDescription}", getUserResult.TopError.Description);
            return getUserResult.Errors;
        }

        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(
            r=>r.UserId == userId
            && r.Token == query.RefreshToken,
            ct);

        if (refreshToken is null
            || refreshToken.ExpiresOnUtc < DateTimeOffset.UtcNow)
        {
            logger.LogWarning("Invalid refresh token for user {UserId}", userId);

            return IdentityApplicationErrors.RefreshTokenExpired;
        }

        var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(getUserResult.Value, ct);
        
        if (generateTokenResult.IsFailure)
        {
            logger.LogError("Generate Jwt Token error occurred: {ErrorDescription}", getUserResult.TopError.Description);

            return generateTokenResult.Errors;
        }

 
        return generateTokenResult.Value;
    }
}