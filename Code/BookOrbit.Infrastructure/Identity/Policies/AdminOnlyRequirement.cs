namespace BookOrbit.Infrastructure.Identity.Policies;
public class AdminOnlyRequirement : IAuthorizationRequirement;


public class AdminOnlyHandler(
    ILogger<AdminOnlyHandler> logger,
    ICurrentUser currentUser) : AuthorizationHandler<AdminOnlyRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminOnlyRequirement requirement)
    {
        var userId = currentUser.Id;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return Task.CompletedTask;
        }

        var isAdmin = currentUser.IsInRole(IdentityRoles.admin.ToString());//Look inside token , doesnt open database

        if (!isAdmin)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: user {UserId} is not Admin", userId);
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}