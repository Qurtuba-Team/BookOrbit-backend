namespace BookOrbit.Application.Features.Students.Queries.GetStudentPublicProfileById;

public record GetStudentPublicProfileByIdQuery(Guid StudentId) : ICachedQuery<Result<StudentDto>>
{
    public string CacheKey => StudentCachingConstants.StudentProfileKey(StudentId);

    public string[] Tags => [StudentCachingConstants.StudentTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(StudentCachingConstants.ExpirationInMinutes);
}