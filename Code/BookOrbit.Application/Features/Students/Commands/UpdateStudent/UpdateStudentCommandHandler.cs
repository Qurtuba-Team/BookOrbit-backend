using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;
using BookOrbit.Domain.PointTransactions.ValueObjects;

namespace BookOrbit.Application.Features.Students.Commands.UpdateStudent;

public class UpdateStudentCommandHandler
    (ILogger<UpdateStudentCommandHandler>logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<UpdateStudentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateStudentCommand command, CancellationToken ct)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s=>s.Id==command.Id,ct);

        if(student is null)
        {
            logger.LogWarning("Student {StudentId} not found for update.", command.Id);

            return StudentApplicationErrors.NotFoundById;
        }

        var nameCreationResult = StudentName.Create(command.Name);

        if (nameCreationResult.IsFailure)
            return nameCreationResult.Errors;

        var updateResult = student.Update(
            nameCreationResult.Value,
            command.personalPhotoFileName);

        if (updateResult.IsFailure)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(StudentCachingConstants.StudentTag, ct);


        logger.LogInformation(
            "Student updated successfully. Id: {StudentId}, Name: {Name}",
            student.Id,
            student.Name);

        return Result.Updated;
    }
}