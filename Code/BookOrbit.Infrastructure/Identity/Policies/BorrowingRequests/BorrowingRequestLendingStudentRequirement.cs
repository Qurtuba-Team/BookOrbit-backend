namespace BookOrbit.Infrastructure.Identity.Policies.BorrowingRequests;
public class BorrowingRequestLendingStudentRequirement : IAuthorizationRequirement;

public class BorrowingRequestLendingStudentHandler(
    ILogger<BorrowingRequestLendingStudentHandler> logger,
    ICurrentUser currentUser,
    IAppDbContext dbContext)
    : AuthorizationHandler<BorrowingRequestLendingStudentRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BorrowingRequestLendingStudentRequirement requirement)
    {
        var userId = currentUser.Id;
        var borrowingRequestIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "borrowingRequestId");

        if(borrowingRequestIdClaim is null)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: borrowingRequestId claim not found");
            return;
        }

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
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
        var isLendingStudent = await dbContext.BorrowingRequests
            .Where(br => (br.LendingRecord!.BookCopy!.Owner!.UserId == userId) && (br.Id.ToString() == borrowingRequestIdClaim.Value))
            .AnyAsync();

        if (!isLendingStudent)
        {
            logger.LogWarning("Authorization failed: user is not the lending student");
            context.Fail();
            return;
        }
        context.Succeed(requirement);
    }
}