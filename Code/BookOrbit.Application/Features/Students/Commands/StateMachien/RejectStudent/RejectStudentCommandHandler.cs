namespace BookOrbit.Application.Features.Students.Commands.StateMachien.RejectStudent;
public class RejectStudentCommandHandler
    (IAppDbContext context,
    ILogger<RejectStudentCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<RejectStudentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RejectStudentCommand command, CancellationToken ct)
    {

        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == command.StudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found for rejecting.", command.StudentId);

            return StudentApplicationErrors.NotFoundById;
        }

        var rejectResult = student.MarkAsRejected();

        if (rejectResult.IsFailure)
            return rejectResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(StudentCachingConstants.StudentTag, ct);

        logger.LogInformation("Student {studentId} has been rejected", student.Id);

        return Result.Updated;

    }
}
