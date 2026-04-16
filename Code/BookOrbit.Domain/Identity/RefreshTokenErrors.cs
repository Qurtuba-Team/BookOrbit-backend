namespace BookOrbit.Domain.Identity;

public static class RefreshTokenErrors
{
    private const string ClassName = nameof(RefreshToken);
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    static public readonly Error TokenRequired = DomainCommonErrors.RequiredProp(ClassName, "Token", "Token");
    static public readonly Error UserIdRequired = DomainCommonErrors.RequiredProp(ClassName, "UserId", "User Id");
    static public readonly Error InvalidExpirationDate = DomainCommonErrors.InvalidProp(ClassName, "ExprirationDate", "Expiration Date");
    static public readonly Error AlreadRevoked = DomainCommonErrors.CustomValidation(ClassName, "TokenAlreadyRevoked", "Token Already Revoked");
    static public readonly Error AlreadyUsed = DomainCommonErrors.CustomValidation(ClassName, "TokenAlreadyUsed", "Token Already Used");
    static public readonly Error CannotUseRevokedToken = DomainCommonErrors.CustomValidation(ClassName, "CannotUseRevokedToken", "Cannot mark a token with [use] while its revoked");

}