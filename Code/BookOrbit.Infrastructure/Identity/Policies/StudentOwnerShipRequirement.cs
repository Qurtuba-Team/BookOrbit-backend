namespace BookOrbit.Infrastructure.Identity.Policies;
public class StudentOwnerShipRequirement : IAuthorizationRequirement;

public class StudentOwnerShipHandler(
    ILogger<StudentOwnerShipHandler> logger,
    IAppDbContext dbContext,
    IRouteParameterService routeParameterService,
    ICurrentUser currentUser) : AuthorizationHandler<StudentOwnerShipRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnerShipRequirement requirement)
    {
        var userId = currentUser.Id;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Authorization failed: userId not found in token");
            context.Fail();
            return;
        }

        //Admin Bybass
        if (currentUser.IsInRole(IdentityRoles.admin.ToString()))
        {
            context.Succeed(requirement);
            return;
        }

        if (!currentUser.IsInRole(IdentityRoles.student.ToString()))
        {
            logger.LogWarning("Authorization failed: user is not Student/Admin");
            context.Fail();
            return;
        }

        var studentIdRouteValueResult = routeParameterService.GetRouteParameter("studentId");

        if (studentIdRouteValueResult.IsFailure)
        {
            logger.LogWarning("Authorization failed: studentId route value not found");
            context.Fail();
            return;
        }

        if (!Guid.TryParse(studentIdRouteValueResult.Value, out var routeStudentId))
        {
            logger.LogWarning("Authorization failed: invalid or missing route id");
            context.Fail();
            return;
        }
        

        var isOwner = await dbContext.Students
            .AnyAsync(s => s.Id == routeStudentId && s.UserId == userId);

        if (!isOwner)
        {
            logger.LogWarning("Authorization failed: user is not owner of this resource");
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}