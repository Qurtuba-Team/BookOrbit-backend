
namespace BookOrbit.Infrastructure.Identity.Policies;
public class RegisteredUserOwnershipRequirement : IAuthorizationRequirement;


public class RegisteredUserOwnershipHandler(
    ILogger<RegisteredUserOwnershipHandler>logger,
    IHttpContextAccessor contextAccessor,
    ICurrentUser currentUser) : AuthorizationHandler<RegisteredUserOwnershipRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RegisteredUserOwnershipRequirement requirement)
    {
        var userId = currentUser.Id;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Authorization failed: userId not found in token");
            context.Fail();
            return Task.CompletedTask;
        }

        //Admin Bybass
        if (currentUser.IsInRole(IdentityRoles.admin.ToString()))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var usertIdRouteValue = contextAccessor.HttpContext?
            .Request.RouteValues["id"]?.ToString();

        var isOwner = userId == usertIdRouteValue;

        if (!isOwner)
        {
            logger.LogWarning("Authorization failed: user is not owner of this resource");
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
