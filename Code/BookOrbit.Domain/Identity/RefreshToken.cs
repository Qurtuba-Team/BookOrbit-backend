namespace BookOrbit.Domain.Identity;

public sealed class RefreshToken : Entity, IAuditableEntity
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModifiedUtc { get; set; }
    public string? LastModifiedBy { get; set; }

    public string? Token { get; }
    public string? UserId { get; }
    public DateTimeOffset ExpiresOnUtc { get; }

    private RefreshToken()
    { }

    private RefreshToken(
        Guid id,
        string? token,
        string? userId,
        DateTimeOffset expiresOnUtc)
        : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
    }

    public static Result<RefreshToken> Create(
        Guid id, 
        string? token, 
        string? userId, 
        DateTimeOffset expiresOnUtc)
    {
        if (id == Guid.Empty)
        {
            return RefreshTokenErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return RefreshTokenErrors.TokenRequired;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return RefreshTokenErrors.UserIdRequired;
        }

        if (expiresOnUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenErrors.InvalidExpirationDate;
        }

        return new RefreshToken(
            id,
            token,
            userId,
            expiresOnUtc);
    }
}