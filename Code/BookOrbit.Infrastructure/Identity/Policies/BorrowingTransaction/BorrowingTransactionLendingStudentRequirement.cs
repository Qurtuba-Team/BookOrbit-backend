namespace BookOrbit.Infrastructure.Identity.Policies.BorrowingTransaction;
public class BorrowingTransactionLendingStudentRequirement : IAuthorizationRequirement;

public class BorrowingTransactionLendingStudentHandler(
    ILogger<BorrowingTransactionLendingStudentHandler> logger,
    ICurrentUser currentUser,
    IAppDbContext dbContext)
    : AuthorizationHandler<BorrowingTransactionLendingStudentRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BorrowingTransactionLendingStudentRequirement requirement)
    {
        var userId = currentUser.Id;
        var borrowingTransactionIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "borrowingTransactionId");
        if (borrowingTransactionIdClaim is null)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: borrowingTransactionId claim not found");
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
        var isLendingStudent = await dbContext.BorrowingTransactions
            .Where(bt => (bt.LenderStudent!.UserId == userId) && (bt.Id.ToString() == borrowingTransactionIdClaim.Value))
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