namespace BookOrbit.Application.Features.Students.Queries.GetStudents;

public record GetStudentsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? SortColumn = QuerySettings.DefaultSortColumn,
    string? SortDirection = QuerySettings.DefaultSortDirection,
    List<StudentState>? States = null,
    bool? EmailConfirmed = null) : IPagedQuery<StudentListItemDto>
{
    public string CacheKey => StudentCachingConstants.StudentListKey(this);

    public string[] Tags => [StudentCachingConstants.StudentTag];

    public TimeSpan Expiration => TimeSpan.FromMinutes(StudentCachingConstants.ExpirationInMinutes);
}