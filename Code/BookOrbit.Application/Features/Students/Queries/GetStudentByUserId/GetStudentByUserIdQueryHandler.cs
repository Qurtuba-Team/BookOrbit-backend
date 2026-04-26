namespace BookOrbit.Application.Features.Students.Queries.GetStudentByUserId;
public class GetStudentByUserIdQueryHandler(
    IAppDbContext context)
    : IRequestHandler<GetStudentByUserIdQuery, Result<StudentDtoWithContactInfo>>
{
    public async Task<Result<StudentDtoWithContactInfo>> Handle(
        GetStudentByUserIdQuery query,
        CancellationToken ct)
    {
        var student = await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == query.UserId, ct);

        if (student is null)
        {
            return StudentApplicationErrors.NotFoundByUserId;
        }

        return StudentDtoWithContactInfo.FromEntity(student);
    }
}

