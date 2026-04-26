namespace BookOrbit.Infrastructure.Identity.Policies.BorrowingTransaction;
public class BorrowingTransactionBorrowingStudentRequirement : IAuthorizationRequirement;

public class BorrowingTransactionBorrowingStudentHandler(
    ILogger<BorrowingTransactionBorrowingStudentHandler> logger,
    ICurrentUser currentUser,
    IRouteParameterService routeParameterService,
    IAppDbContext dbContext)
    : AuthorizationHandler<BorrowingTransactionBorrowingStudentRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BorrowingTransactionBorrowingStudentRequirement requirement)
    {
        var userId = currentUser.Id;
        var borrowingTransactionIdResult = routeParameterService.GetRouteParameter("borrowingTransactionId");
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return;
        }
        if (borrowingTransactionIdResult.IsFailure)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: borrowingTransactionId route value not found");
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

        var isBorrowingStudent = await dbContext.BorrowingTransactions
            .Where(bt => (bt.BorrowingRequest!.BorrowingStudent!.UserId == userId) && (bt.Id == routeBorrowingTransactionId))
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