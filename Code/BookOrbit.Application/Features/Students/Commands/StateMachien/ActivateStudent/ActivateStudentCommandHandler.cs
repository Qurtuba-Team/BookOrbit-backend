namespace BookOrbit.Application.Features.Students.Commands.StateMachien.ActivateStudent;
public class ActivateStudentCommandHandler(
    IAppDbContext context,
    ILogger<ActivateStudentCommandHandler> logger,
    HybridCache cache) : IRequestHandler<ActivateStudentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(ActivateStudentCommand command, CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == command.StudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found for activation.", command.StudentId);

            return StudentApplicationErrors.NotFoundById;
        }

        var activationResult = student.MarkAsActivated();

        if (activationResult.IsFailure)
            return activationResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(StudentCachingConstants.StudentTag, ct);

        logger.LogInformation("Student {studentId} has been activated", student.Id);

        return Result.Updated;
    }
}