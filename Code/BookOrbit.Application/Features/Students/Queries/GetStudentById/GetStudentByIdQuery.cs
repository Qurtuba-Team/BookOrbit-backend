namespace BookOrbit.Application.Features.Students.Queries.GetStudentById;

public record GetStudentByIdQuery(Guid StudentId) : ICachedQuery<Result<StudentDtoWithContactInfo>>
{
    public string CacheKey => StudentCachingConstants.StudentKey(StudentId);

    public string[] Tags => [StudentCachingConstants.StudentTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(StudentCachingConstants.ExpirationInMinutes);
}