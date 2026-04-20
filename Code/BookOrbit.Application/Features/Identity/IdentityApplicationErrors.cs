namespace BookOrbit.Application.Features.Identity;
static public class IdentityApplicationErrors
{
    private const string ClassName = "User";
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "UserId", "User Id");
    static public readonly Error EmailRequired = DomainCommonErrors.RequiredProp(ClassName, "Email", "Email");
    static public readonly Error PasswordRequired = DomainCommonErrors.RequiredProp(ClassName, "Password", "Password");
    static public readonly Error InvalidEmail = DomainCommonErrors.InvalidProp(ClassName, "Email", "Email");
    static public readonly Error InvalidPassword = DomainCommonErrors.InvalidProp(ClassName, "Password", "Password");
    static public readonly Error ExpiredAccessTokenInvalid = DomainCommonErrors.InvalidProp(ClassName, "AccessToken", "Access Token");
    static public readonly Error UserIdClaimInvalid = DomainCommonErrors.InvalidProp(ClassName, "UserIdClaim", "User Id Claim");
    static public readonly Error RefreshTokenExpired = DomainCommonErrors.CustomUnAuthorized(ClassName, "RefreshTokenExpired", "This refresh token has expired");
    static public readonly Error EmailConfirmationTokenRequired = DomainCommonErrors.RequiredProp(ClassName, "EmailConfirmationRefreshToken", "Email Confirmation Refresh Token");
    static public readonly Error PasswordResetTokenRequired = DomainCommonErrors.RequiredProp(ClassName, "PasswordResetToken", "Password Reset Token");
}