namespace BookOrbit.Application.Features.Students.Commands.StateMachien.ApproveStudent;
public class ApproveStudentCommandHandler (
    IAppDbContext context,
    TimeProvider timeProvider,
    IIdentityService identityService,
    IMaskingService maskingService,
    ILogger<ApproveStudentCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<ApproveStudentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        ApproveStudentCommand command,
        CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == command.StudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found for approving.", command.StudentId);

            return StudentApplicationErrors.NotFoundById;
        }
        var emailConfirmedResult = await identityService.IsEmailConfirmedAsync(student.UserId,ct);

        if (emailConfirmedResult.IsFailure)
            return emailConfirmedResult.Errors;

        if (!emailConfirmedResult.Value)
        {
            logger.LogWarning("Student {StudentId} cannot be approved because email {MaskedEmail} is not confirmed.", command.StudentId, maskingService.MaskEmail(student.UniversityMail.Value));

            return StudentApplicationErrors.StateTransitionFailedEmailNotConfirmed;
        }

        var approveResult = student.MarkAsApproved(timeProvider.GetUtcNow());

        if (approveResult.IsFailure)
            return approveResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(StudentCachingConstants.StudentTag, ct);

        logger.LogInformation("Student {studentId} has been approved", student.Id);

        return Result.Updated;
    }
}
