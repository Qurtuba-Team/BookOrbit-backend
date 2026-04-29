namespace BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;

using System.Security.Claims;
using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Features.Identity.Dtos;
using BookOrbit.Domain.Common.Results;

internal sealed class FakeTokenProvider : ITokenProvider
{
    public Result<TokenDto> TokenResult { get; set; } = new TokenDto
    {
        AccessToken = "access",
        RefreshToken = "refresh",
        ExpiresOnUtc = DateTime.UtcNow.AddMinutes(15)
    };

    public ClaimsPrincipal? Principal { get; set; }

    public Task<Result<TokenDto>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct)
        => Task.FromResult(TokenResult);

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string ExpiredAccessToken)
        => Principal;
}
