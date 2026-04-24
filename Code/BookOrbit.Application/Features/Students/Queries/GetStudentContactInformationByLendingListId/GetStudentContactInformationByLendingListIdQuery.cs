namespace BookOrbit.Application.Features.Students.Queries.GetStudentContactInformationByLendingListId;

public record GetStudentContactInformationByLendingListIdQuery(Guid LendingListId, Guid StudentId) : ICachedQuery<Result<StudentContactInformationDto>>
{
    public string CacheKey => StudentCachingConstants.StudentContactInformationKeyByLendingListId(LendingListId);

    public string[] Tags => [StudentCachingConstants.StudentTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(StudentCachingConstants.ExpirationInMinutes);
}
