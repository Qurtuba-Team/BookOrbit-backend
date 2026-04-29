
namespace BookOrbit.Infrastructure.Identity;

public class IdentityService(
    UserManager<AppUser> userManager,
    ILogger<IdentityService> logger,
    IMaskingService maskingService) : IIdentityService
{
    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return InfrastructureIdentityErrors.InvalidLoginAttempt;

        //User Cannot Access His Email Without Confirming , 
        //He Can Access Confirmation Endpoint To Resend Confirmation Email But He Cannot Access Any Other Endpoint Before Confirming His Email

        if (!user.EmailConfirmed)
            return InfrastructureIdentityErrors.EmailNotConfirmed;

        if (!await userManager.CheckPasswordAsync(user, password))
            return InfrastructureIdentityErrors.InvalidLoginAttempt;


        return new AppUserDto(
            user.UserName ?? "Unknown",
            user.Id,
            user.Email!,
            await userManager.GetRolesAsync(user),
            await userManager.GetClaimsAsync(user),
            user.EmailConfirmed);
    }

    public async Task<Result<string>> CreateStudent(string Name,string email, string password, CancellationToken ct = default)
    {
        var user = new AppUser
        {
            UserName = Name,
            Email = email,
            EmailConfirmed = false,
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            logger.LogError(
                "Failed to create user with email {Email}. Errors: {Errors}",
                maskingService.MaskEmail(email),
                string.Join(" | ", createResult.Errors.Select(e => e.Description)));

            return InfrastructureIdentityErrors.UserCreationFaild;
        }

        string role = IdentityRoles.student.ToString();
        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            logger.LogError(
                "Failed to add user with email {Email} to role {Role}. Errors: {Errors}",
                maskingService.MaskEmail(email),
                role,
                string.Join(" | ", roleResult.Errors.Select(e => e.Description)));

            return InfrastructureIdentityErrors.UserCreationFaild;
        }

        return user.Id;
    }

    public async Task<Result<Deleted>> DeleteUserByIdAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Deleted;

        var deleteResult = await userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            logger.LogError(
                "Failed to delete user with ID {UserId}. Errors: {Errors}",
                userId,
                string.Join(" | ", deleteResult.Errors.Select(e => e.Description)));

            return InfrastructureIdentityErrors.UserDeletionFailed;
        }
        return Result.Deleted;
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user is null)
            return InfrastructureIdentityErrors.UserNotFoundById;

        return new AppUserDto(
            user.UserName??"Unknown",
            user.Id,
            user.Email!,
            await userManager.GetRolesAsync(user),
            await userManager.GetClaimsAsync(user),
            user.EmailConfirmed);
    }

    public async Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<bool> UserEmailExists(string email, CancellationToken ct = default) =>
         await userManager.Users.AnyAsync(u => u.Email == email, ct);

    public async Task<Result<bool>> IsEmailConfirmedAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return InfrastructureIdentityErrors.UserNotFoundById;

        return user.EmailConfirmed;
    }
}
