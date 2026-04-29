
namespace BookOrbit.Infrastructure.Services;
public class EmailConfirmationService(
    UserManager<AppUser> userManager,
    ILogger<EmailConfirmationService> logger) : IEmailConfirmationService
{
    public async Task<Result<EmailConfirmationTokenDto>> GenerateEmailConfirmationTokenAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return InfrastructureIdentityErrors.UserNotFoundByEmail;

        if (user.EmailConfirmed)
            return InfrastructureIdentityErrors.EmailAlreadyConfirmed;

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return new EmailConfirmationTokenDto(encodedToken, user.Email!);
    }

    public async Task<Result<Updated>> ConfirmEmailAsync(string email, string encodedToken, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return InfrastructureIdentityErrors.UserNotFoundByEmail;

        if (user.EmailConfirmed)
            return InfrastructureIdentityErrors.EmailAlreadyConfirmed;

        string decodedToken;

        try
        {
            decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(encodedToken));
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to decode the email confirmation token for email: {Email}", email);
            return InfrastructureIdentityErrors.InvalidEmailConfirmationToken;
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            return InfrastructureIdentityErrors.InvalidEmailConfirmationToken;

        return Result.Updated;
    }
}
