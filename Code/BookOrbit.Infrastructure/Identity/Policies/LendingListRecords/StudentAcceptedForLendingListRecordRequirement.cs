
namespace BookOrbit.Infrastructure.Identity.Policies.LendingListRecords;
public class StudentAcceptedForLendingListRecordRequirement : IAuthorizationRequirement;

public class StudentAcceptedForLendingListRecordHandler(
    ILogger<StudentAcceptedForLendingListRecordHandler> logger,
    ICurrentUser currentUser,
    IRouteParameterService routeParameterService,
    IAppDbContext dbContext) : AuthorizationHandler<StudentAcceptedForLendingListRecordRequirement>
{
    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentAcceptedForLendingListRecordRequirement requirement)
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

        var isStudentAccepted = await dbContext.BorrowingRequests
            .Where(br=>
            (br.LendingRecordId == routeLendingListRecordId)
            && 
            (br.BorrowingStudent!.UserId == userId)
            && (br.State == Domain.BorrowingRequests.Enums.BorrowingRequestState.Accepted)
            )
            .AnyAsync();

        if (!isStudentAccepted)
        {
            logger.LogWarning("Authorization failed: user is not accepted for the lending list record");
            context.Fail();
            return;
        }
        context.Succeed(requirement);

    }
}