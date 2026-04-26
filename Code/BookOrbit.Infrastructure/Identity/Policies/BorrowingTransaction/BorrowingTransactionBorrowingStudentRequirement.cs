namespace BookOrbit.Infrastructure.Identity.Policies.BorrowingTransaction;
public class BorrowingTransactionBorrowingStudentRequirement : IAuthorizationRequirement;

public class BorrowingTransactionBorrowingStudentHandler(
    ILogger<BorrowingTransactionBorrowingStudentHandler> logger,
    ICurrentUser currentUser,
    IAppDbContext dbContext)
    : AuthorizationHandler<BorrowingTransactionBorrowingStudentRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BorrowingTransactionBorrowingStudentRequirement requirement)
    {
        var userId = currentUser.Id;
        var borrowingTransactionIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "borrowingTransactionId");
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return;
        }
        if (borrowingTransactionIdClaim is null)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: borrowingTransactionId claim not found");
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
        var isBorrowingStudent = await dbContext.BorrowingTransactions
            .Where(bt => (bt.BorrowingRequest!.BorrowingStudent!.UserId == userId) && (bt.Id.ToString() == borrowingTransactionIdClaim.Value))
            .AnyAsync();
        if (!isBorrowingStudent)
        {
            logger.LogWarning("Authorization failed: user is not the borrowing student");
            context.Fail();
            return;
        }
        context.Succeed(requirement);
    }
}