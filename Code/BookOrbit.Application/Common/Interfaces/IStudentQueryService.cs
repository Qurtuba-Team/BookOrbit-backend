namespace BookOrbit.Application.Common.Interfaces;
public interface IStudentQueryService
{
    IQueryable<Student> GetStudentsWithEmailStatus(IQueryable<Student> query, bool EmailConfirmed);
}
