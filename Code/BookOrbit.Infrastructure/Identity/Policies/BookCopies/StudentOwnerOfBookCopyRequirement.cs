namespace BookOrbit.Infrastructure.Identity.Policies.BookCopies;
public class StudentOwnerOfBookCopyRequirement : IAuthorizationRequirement;

public class StudentOwnerOfBookCopyHandler(
    ILogger<StudentOwnerOfBookCopyHandler> logger,
    ICurrentUser currentUser,
    IAppDbContext dbContext)
    : AuthorizationHandler<StudentOwnerOfBookCopyRequirement>
{
    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnerOfBookCopyRequirement requirement)
    {
        var userId = currentUser.Id;
        var bookCopyIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "bookCopyId");
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            logger.LogWarning("Authorization failed: userId not found in token");
            return;
        }
        if (bookCopyIdClaim is null)
        {
            context.Fail();
            logger.LogWarning("Authorization failed: bookCopyId claim not found");
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
        var isOwnerStudent = await dbContext.BookCopies
            .Where(bc => (bc.Owner!.UserId == userId) && (bc.Id.ToString() == bookCopyIdClaim.Value))
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