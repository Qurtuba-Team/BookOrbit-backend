namespace BookOrbit.Infrastructure.Identity.Policies;
public class StudentOnlyRequirement : IAuthorizationRequirement;

public class StudentOnlyHandler(
    ILogger<StudentOnlyHandler> logger,
    ICurrentUser currentUser) : AuthorizationHandler<StudentOnlyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentOnlyRequirement requirement)
    {
        var userId = currentUser.Id;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Authorization failed: userId not found in token");
            context.Fail();
            return Task.CompletedTask;
        }

        //NO ADMIN Bypass

        if (!currentUser.IsInRole(IdentityRoles.student.ToString()))
        {
            logger.LogWarning("Authorization failed: user is not Student");
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
