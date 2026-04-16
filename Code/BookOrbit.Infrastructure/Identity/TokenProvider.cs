namespace BookOrbit.Infrastructure.Identity;

public class TokenProvider
    (IAppDbContext context,
    IConfiguration configuration): ITokenProvider
{
    public async Task<Result<TokenDto>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct)
    {
        var tokenResult = await CreateTokenAsync(user, ct);

        if (tokenResult.IsFailure)
        {
            return tokenResult.Errors;
        }

        return tokenResult.Value;
    }

    private async Task<Result<TokenDto>> CreateTokenAsync(AppUserDto user, CancellationToken ct)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        var issure = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var key = jwtSettings["Key"]!;
        var accessTokenExpritesInMinutes = int.Parse(jwtSettings["AccessTokenExpirationInMinutes"]!);

        var expires = DateTime.UtcNow.AddMinutes(accessTokenExpritesInMinutes);

        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier,user.UserId),
            new(ClaimTypes.Email,user.Email),
        };

        foreach (var claim in user.Claims) 
        {
            claims.Add(claim);
        }

        foreach(var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issure,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(descriptor); // new token

        //delete old refresh token
        await context.RefreshTokens.
            Where(rt => rt.UserId == user.UserId)
            .ExecuteDeleteAsync(ct); //Delete NOW!!

        //create new one

        var refreshTokenExpritesInMinutes = int.Parse(jwtSettings["RefreshTokenExpirationInDays"]!);


        var refreshTokenCreationResult = RefreshToken.Create(
            Guid.NewGuid(),
            GenerateRefreshToken(),
            user.UserId,
            DateTime.UtcNow.AddDays(refreshTokenExpritesInMinutes));

        if (refreshTokenCreationResult.IsFailure)
        {
            return refreshTokenCreationResult.Errors;
        }

        await context.RefreshTokens.AddAsync(refreshTokenCreationResult.Value, ct);

        await context.SaveChangesAsync(ct);

        return new TokenDto()
        {
            AccessToken = tokenHandler.WriteToken(securityToken),
            RefreshToken = refreshTokenCreationResult.Value.Token,
            ExpiresOnUtc = expires
        };
    }
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }


    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string ExpiredAccessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["JwtSettings:Audience"],
            ValidateLifetime = false, // Ignore token expiration
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(ExpiredAccessToken, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            return null;

        return principal;
    }
}

