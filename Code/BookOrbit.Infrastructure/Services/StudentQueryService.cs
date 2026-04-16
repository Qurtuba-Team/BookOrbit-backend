namespace BookOrbit.Infrastructure.Services;
public class StudentQueryService(AppDbContext context) : IStudentQueryService
{
    public IQueryable<Student> GetStudentsWithEmailStatus(IQueryable<Student> query, bool EmailConfirmed)
    {
        query = query.Join(context.Users,
                student => student.UserId,
                user => user.Id,
                (student, user) => new { student, user })
            .Where(x => x.user.EmailConfirmed == EmailConfirmed)
            .Select(x => x.student);

        return query;
    }
}