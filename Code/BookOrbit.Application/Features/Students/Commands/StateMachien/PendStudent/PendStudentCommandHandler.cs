
namespace BookOrbit.Application.Features.Students.Commands.StateMachien.PendStudent;
public class PendStudentCommandHandler(
    ILogger<PendStudentCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<PendStudentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(PendStudentCommand command, CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == command.StudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found for pending.", command.StudentId);

            return StudentApplicationErrors.NotFoundById;
        }

        var pendResult = student.Pend();

        if (pendResult.IsFailure)
            return pendResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(StudentCachingConstants.StudentTag,ct);

        logger.LogInformation("Student {studentId} has been pended", student.Id);

        return Result.Updated;
    }
}