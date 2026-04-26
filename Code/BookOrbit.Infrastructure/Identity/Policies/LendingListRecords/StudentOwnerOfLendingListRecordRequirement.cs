namespace BookOrbit.Infrastructure.Identity.Policies.LendingListRecords;
public class StudentOwnerOfLendingListRecordRequirement : IAuthorizationRequirement;

public class StudentOwnerOfLendingListRecordHandler(
    ILogger<StudentOwnerOfLendingListRecordHandler> logger,
    ICurrentUser currentUser,
    IAppDbContext dbContext)
    : AuthorizationHandler<StudentOwnerOfLendingListRecordRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnerOfLendingListRecordRequirement requirement)
    {
        var userId = currentUser.Id;
        var lendingListRecordIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "lendingListRecordId");

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return;
        }
        if (lendingListRecordIdClaim is null)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: lendingListRecordId claim not found");
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
        var isOwnerStudent = await dbContext.LendingListRecords
            .Where(llr => (llr.BookCopy!.Owner!.UserId == userId) && (llr.Id.ToString() == lendingListRecordIdClaim.Value))
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