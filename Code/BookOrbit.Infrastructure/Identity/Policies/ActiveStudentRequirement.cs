
namespace BookOrbit.Infrastructure.Identity.Policies;
public class ActiveStudentRequirement:IAuthorizationRequirement;

public class ActiveStudentHandler(
    ILogger<ActiveUserHandler> logger,
    ICurrentUser currentUser,
    IAppDbContext dbContext)
    : AuthorizationHandler<ActiveStudentRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveStudentRequirement requirement)
    {
        var userId = currentUser.Id;


        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return;
        }

        //Admin Bypass
        if(currentUser.IsInRole(IdentityRoles.admin.ToString()))
        {
            context.Succeed(requirement);
            return;
        }

        if (!currentUser.IsInRole(IdentityRoles.student.ToString()))
        {
            logger.LogWarning("Authorization failed: user is not Student");
            context.Fail();
            return;
        }

        var isActive = await dbContext.Students.AnyAsync(
            s => s.UserId == userId && s.State == Domain.Students.Enums.StudentState.Active);

        if (!isActive)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId : [{userId}] not found in system or isnt active", userId);
            return;
        }

        context.Succeed(requirement);
        return;

    }
}
