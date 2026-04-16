namespace BookOrbit.Application.Features.Students.Commands.StateMachien.UnBanStudent;
public class UnBanStudentCommandHandler
    (IAppDbContext context,
    ILogger<UnBanStudentCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<UnBanStudentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UnBanStudentCommand command, CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == command.StudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found for un banning.", command.StudentId);

            return StudentApplicationErrors.NotFoundById;
        }

        var unBanResult = student.UnBan();

        if (unBanResult.IsFailure)
            return unBanResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(StudentCachingConstants.StudentTag, ct);

        logger.LogInformation("Student {studentId} has been un banned", student.Id);

        return Result.Updated;

    }
}