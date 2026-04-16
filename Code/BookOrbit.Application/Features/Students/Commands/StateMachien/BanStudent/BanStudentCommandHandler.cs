namespace BookOrbit.Application.Features.Students.Commands.StateMachien.BanStudent;
public class BanStudentCommandHandler(
    IAppDbContext context,
    ILogger<BanStudentCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<BanStudentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(BanStudentCommand command, CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == command.StudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found for banning.", command.StudentId);

            return StudentApplicationErrors.NotFoundById;
        }

        var banResult = student.Ban();

        if (banResult.IsFailure)
            return banResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(StudentCachingConstants.StudentTag, ct);

        logger.LogInformation("Student {studentId} has been banned", student.Id);

        return Result.Updated;
    }

}