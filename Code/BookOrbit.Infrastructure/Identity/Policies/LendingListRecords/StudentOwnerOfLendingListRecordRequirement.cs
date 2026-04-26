namespace BookOrbit.Infrastructure.Identity.Policies.LendingListRecords;
public class StudentOwnerOfLendingListRecordRequirement : IAuthorizationRequirement;

public class StudentOwnerOfLendingListRecordHandler(
    ILogger<StudentOwnerOfLendingListRecordHandler> logger,
    ICurrentUser currentUser,
    IRouteParameterService routeParameterService,
    IAppDbContext dbContext)
    : AuthorizationHandler<StudentOwnerOfLendingListRecordRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnerOfLendingListRecordRequirement requirement)
    {
        var userId = currentUser.Id;
        var lendingListRecordIdResult = routeParameterService.GetRouteParameter("lendingListRecordId");

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return;
        }
        if (lendingListRecordIdResult.IsFailure)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: lendingListRecordId route value not found");
            return;
        }
        //Admin Bypass
        if (currentUser.IsInRole(IdentityRoles.admin.ToString()))
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


        if (!Guid.TryParse(lendingListRecordIdResult.Value, out var routeLendingListRecordId))
        {
            logger.LogWarning("Authorization failed: invalid or missing route id");
            context.Fail();
            return;
        }

        var isOwnerStudent = await dbContext.LendingListRecords
            .Where(llr => (llr.BookCopy!.Owner!.UserId == userId) && (llr.Id == routeLendingListRecordId))
            .AnyAsync();
        if (!isOwnerStudent)
        {
            logger.LogWarning("Authorization failed: user is not the owner student of the lending list record");
            context.Fail();
            return;
        }
        context.Succeed(requirement);
    }
}