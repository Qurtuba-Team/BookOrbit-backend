
namespace BookOrbit.Infrastructure.Identity.Policies;
public class RegisteredUserOwnershipRequirement : IAuthorizationRequirement;


public class RegisteredUserOwnershipHandler(
    ILogger<RegisteredUserOwnershipHandler>logger,
    IRouteParameterService routeParameterService,
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

        var userIdRouteValueResult = routeParameterService.GetRouteParameter("userId");

        if (userIdRouteValueResult.IsFailure)
        {
            logger.LogWarning("Authorization failed: userId route value not found");
            context.Fail();
            return Task.CompletedTask;
        }

        var isOwner = userId == userIdRouteValueResult.Value;

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