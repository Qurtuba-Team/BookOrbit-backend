namespace BookOrbit.Infrastructure.Identity.Policies.BorrowingTransaction;
public class BorrowingTransactionRelatedStudentRequirement : IAuthorizationRequirement;

public class BorrowingTransactionRelatedStudentHandler(
    ILogger<BorrowingTransactionRelatedStudentHandler> logger,
    ICurrentUser currentUser,
    IRouteParameterService routeParameterService,
    IAppDbContext dbContext) : AuthorizationHandler<BorrowingTransactionRelatedStudentRequirement>
{
    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, BorrowingTransactionRelatedStudentRequirement requirement)
    {
        var userId = currentUser.Id;
        var borrowingTransactionIdResult = routeParameterService.GetRouteParameter("borrowingTransactionId");
        if (borrowingTransactionIdResult.IsFailure)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: borrowingTransactionId route value not found");
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

        if (!Guid.TryParse(borrowingTransactionIdResult.Value, out var routeBorrowingTransactionId))
        {
            logger.LogWarning("Authorization failed: invalid or missing route id");
            context.Fail();
            return;
        }

        var isRelatedStudent = await dbContext.BorrowingTransactions
    .Where(bt =>

    (
    (bt.BorrowingRequest!.BorrowingStudent!.UserId == userId)
    ||
    (bt.LenderStudent!.UserId == userId)
    )
    && (bt.Id == routeBorrowingTransactionId)


    )
    .AnyAsync();
        if (!isRelatedStudent)
        {
            logger.LogWarning("Authorization failed: user is not related to the borrowing transaction");
            context.Fail();
            return;
        }
        context.Succeed(requirement);

    }
}