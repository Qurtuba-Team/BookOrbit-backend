namespace BookOrbit.Infrastructure.Identity.Policies.BorrowingRequests;
public class BorrowingRequestBorrowingStudentRequirement : IAuthorizationRequirement;

public class BorrowingRequestBorrowingStudentHandler(
    ILogger<BorrowingRequestBorrowingStudentHandler> logger,
    ICurrentUser currentUser,
    IRouteParameterService routeParameterService,
    IAppDbContext dbContext)
    : AuthorizationHandler<BorrowingRequestBorrowingStudentRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BorrowingRequestBorrowingStudentRequirement requirement)
    {
        var userId = currentUser.Id;
        var borrowingRequestIdResult = routeParameterService.GetRouteParameter("borrowingRequestId");

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return;
        }

        if (borrowingRequestIdResult.IsFailure)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: borrowingRequestId route value not found");
            return;
        }

        if (!Guid.TryParse(borrowingRequestIdResult.Value, out var routeBorrowingRequestId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: invalid or missing route id");
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

        var isBorrowingStudent = await dbContext.BorrowingRequests
            .Where(br => (br.BorrowingStudent!.UserId == userId) && (br.Id == routeBorrowingRequestId))
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