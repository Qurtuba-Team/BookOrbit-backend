namespace BookOrbit.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenDto>> GenerateJwtTokenAsync(AppUserDto user,CancellationToken ct);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string ExpiredAccessToken);
}