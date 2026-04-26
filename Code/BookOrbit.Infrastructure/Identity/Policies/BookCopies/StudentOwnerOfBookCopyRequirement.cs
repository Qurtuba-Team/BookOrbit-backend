namespace BookOrbit.Infrastructure.Identity.Policies.BookCopies;
public class StudentOwnerOfBookCopyRequirement : IAuthorizationRequirement;

public class StudentOwnerOfBookCopyHandler(
    ILogger<StudentOwnerOfBookCopyHandler> logger,
    ICurrentUser currentUser,
    IRouteParameterService routeParameterService,
    IAppDbContext dbContext)
    : AuthorizationHandler<StudentOwnerOfBookCopyRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnerOfBookCopyRequirement requirement)
    {
        var userId = currentUser.Id;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return;
        }

        var bookCopyIdResult = routeParameterService.GetRouteParameter("bookCopyId");

        if (bookCopyIdResult.IsFailure)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: bookCopyId route value not found");
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

        if (!Guid.TryParse(bookCopyIdResult.Value, out var routeBookCopyId))
        {
            logger.LogWarning("Authorization failed: invalid or missing route id");
            context.Fail();
            return;
        }

        var isOwnerStudent = await dbContext.BookCopies
            .Where(bc => (bc.Owner!.UserId == userId) && (bc.Id == routeBookCopyId))
            .AnyAsync();
        if (!isOwnerStudent)
        {
            logger.LogWarning("Authorization failed: user is not the owner student of the book copy");
            context.Fail();
            return;
        }
        context.Succeed(requirement);
    }
}