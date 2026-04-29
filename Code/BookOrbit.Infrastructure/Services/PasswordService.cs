
namespace BookOrbit.Infrastructure.Services;
public class PasswordService(
    UserManager<AppUser> userManager,
    ILogger<PasswordService> logger) : IPasswordService
{
    public async Task<Result<ResetPasswordTokenDto>> GenerateResetPasswordTokenAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return InfrastructureIdentityErrors.UserNotFoundByEmail;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return new ResetPasswordTokenDto(user.Email!, encodedToken);
    }
   
    public async Task<Result<Success>> ResetPasswordAsync(string email, string encodedToken, string newPassword, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return InfrastructureIdentityErrors.UserNotFoundByEmail;

        string decodedToken;

        try
        {
            decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(encodedToken));
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to decode the password reset token for email: {Email}", email);
            return InfrastructureIdentityErrors.InvalidEmailConfirmationToken;
        }

        var resetPasswordResult = await userManager.ResetPasswordAsync(user, decodedToken, newPassword);

        if (!resetPasswordResult.Succeeded)
            return resetPasswordResult.Errors.Select(e => Error.Conflict(e.Code, e.Description)).ToList();

        return Result.Success;
    }

    public async Task<Result<Success>> ChangePasswordAsync(string email, string oldPassword, string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return InfrastructureIdentityErrors.UserNotFoundByEmail;

        var ChangePasswordResult = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);

        if (!ChangePasswordResult.Succeeded)
            return ChangePasswordResult.Errors.Select(e => Error.Conflict(e.Code, e.Description)).ToList();

        return Result.Success;
    }
}
