namespace BookOrbit.Application.Features.Students.Queries.GetStudentById;

public record GetStudentByIdQuery(Guid StudentId) : ICachedQuery<Result<StudentDto>>
{
    public string CacheKey => StudentCachingConstants.StudentKey(StudentId);

    public string[] Tags => [StudentCachingConstants.StudentTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(StudentCachingConstants.ExpirationInMinutes);
}