

namespace BookOrbit.Application.Features.Students.Queries.GetStudents;

public class GetStudentsQueryHandler(IAppDbContext context, IStudentQueryService studentQueryService)
    :BasePagedQueryHandler<Student, StudentListItemDto, GetStudentsQuery>
{
    protected override Dictionary<
    string,
    Func<IQueryable<Student>, bool, IOrderedQueryable<Student>>>
    SortMappings
    => new()
    {
        ["createdat"] = (query, desc) =>
            desc
                ? query.OrderByDescending(x => x.CreatedAtUtc)
                : query.OrderBy(x => x.CreatedAtUtc),

        ["updatedat"] = (query, desc) =>
            desc
                ? query.OrderByDescending(x => x.LastModifiedUtc)
                : query.OrderBy(x => x.LastModifiedUtc),

        ["name"] = (query, desc) =>
            desc
                ? query.OrderByDescending(x => x.Name.Value)
                : query.OrderBy(x => x.Name.Value),

        ["state"] = (query, desc) =>
            desc
                ? query.OrderByDescending(x => x.State)
                : query.OrderBy(x => x.State),

        ["joindate"] = (query, desc) =>
            desc
                ? query.OrderByDescending(x => x.JoinDateUtc)
                : query.OrderBy(x => x.JoinDateUtc)
    };

    protected override IQueryable<Student> ApplySearch(
        IQueryable<Student> query, GetStudentsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;//no need for filters

        var normalizedName = StudentName.Normalize(searchQuery.SearchTerm);
        var normalizedPhoneNumber = PhoneNumber.Normalize(searchQuery.SearchTerm);
        var normalizedMail = UniversityMail.Normalize(searchQuery.SearchTerm);
        var normalizedTelegramUserId = TelegramUserId.Normalize(searchQuery.SearchTerm);

        query = query.Where(s =>
            s.Name.Value.Contains(normalizedName) ||
            (s.PhoneNumber != null && s.PhoneNumber.Value.Contains(normalizedPhoneNumber)) ||
            s.UniversityMail.Value.Contains(normalizedMail) ||
            (s.TelegramUserId != null && s.TelegramUserId.Value.Contains(normalizedTelegramUserId)));


        return query;
    }

    protected override IQueryable<Student> ApplyFilters(IQueryable<Student> query, GetStudentsQuery searchQuery)
    {
        if(searchQuery.States is not null &&
            searchQuery.States.Count != 0)
                query = query.Where(s => searchQuery.States.Contains(s.State));

        if (searchQuery.EmailConfirmed.HasValue)
            query = studentQueryService.GetStudentsWithEmailStatus(query, searchQuery.EmailConfirmed.Value);

        return query;
    }

    protected override IQueryable<Student> GetBaseQuery()
    {
        return context.Students.AsNoTracking();
    }

    protected override IQueryable<StudentListItemDto> ProjectToDto(IQueryable<Student> query)
    {
        return query.Select(StudentListItemDto.Projection);
    }
}
