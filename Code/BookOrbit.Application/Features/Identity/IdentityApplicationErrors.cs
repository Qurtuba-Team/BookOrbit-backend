namespace BookOrbit.Application.Features.Identity;
static public class IdentityApplicationErrors
{
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(UserErrors.ClassName, "UserId", "User Id");
    static public readonly Error EmailRequired = DomainCommonErrors.RequiredProp(UserErrors.ClassName, "Email", "Email");
    static public readonly Error PasswordRequired = DomainCommonErrors.RequiredProp(UserErrors.ClassName, "Password", "Password");
    static public readonly Error InvalidEmail = DomainCommonErrors.InvalidProp(UserErrors.ClassName, "Email", "Email");
    static public readonly Error InvalidPassword = DomainCommonErrors.InvalidProp(UserErrors.ClassName, "Password", "Password");
    static public readonly Error ExpiredAccessTokenInvalid = DomainCommonErrors.InvalidProp(UserErrors.ClassName, "AccessToken", "Access Token");
    static public readonly Error UserIdClaimInvalid = DomainCommonErrors.InvalidProp(UserErrors.ClassName, "UserIdClaim", "User Id Claim");
    static public readonly Error RefreshTokenExpired = DomainCommonErrors.CustomUnAuthorized(UserErrors.ClassName, "RefreshTokenExpired", "This refresh token has expired");
    static public readonly Error EmailConfirmationTokenRequired = DomainCommonErrors.RequiredProp(UserErrors.ClassName, "EmailConfirmationRefreshToken", "Email Confirmation Refresh Token");
    static public readonly Error PasswordResetTokenRequired = DomainCommonErrors.RequiredProp(UserErrors.ClassName, "PasswordResetToken", "Password Reset Token");
}