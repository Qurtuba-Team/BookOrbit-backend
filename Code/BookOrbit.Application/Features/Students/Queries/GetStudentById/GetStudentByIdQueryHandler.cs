namespace BookOrbit.Application.Features.Students.Queries.GetStudentById;

public class GetStudentByIdQueryHandler
    (ILogger<GetStudentByIdQueryHandler> logger,
    IAppDbContext context)
    : IRequestHandler<GetStudentByIdQuery, Result<StudentDtoWithContactInfo>>
{
    public async Task<Result<StudentDtoWithContactInfo>> Handle(GetStudentByIdQuery query, CancellationToken ct)
    {
        var student = await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.StudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found .", query.StudentId);

            return StudentApplicationErrors.NotFoundById;
        }

        return StudentDtoWithContactInfo.FromEntity(student);
    }
}