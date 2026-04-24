namespace BookOrbit.Application.Features.Students.Queries.GetStudentPublicProfileById;

public class GetStudentPublicProfileByIdQueryHandler
    (ILogger<GetStudentPublicProfileByIdQueryHandler> logger,
    IAppDbContext context)
    : IRequestHandler<GetStudentPublicProfileByIdQuery, Result<StudentDto>>
{
    public async Task<Result<StudentDto>> Handle(GetStudentPublicProfileByIdQuery query, CancellationToken ct)
    {
        var student = await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.StudentId, ct);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found .", query.StudentId);

            return StudentApplicationErrors.NotFoundById;
        }

        return StudentDto.FromEntity(student);
    }
}