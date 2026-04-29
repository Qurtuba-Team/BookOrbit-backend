

namespace BookOrbit.Application.Features.Students.Queries.GetStudents;

public class GetStudentsQueryHandler(IAppDbContext context,IStudentQueryService studentQueryService)
    : IRequestHandler<GetStudentsQuery, Result<PaginatedList<StudentListItemDto>>>
{
    public async Task<Result<PaginatedList<StudentListItemDto>>> Handle(GetStudentsQuery query, CancellationToken ct)
    {
        var studentQuery = context.Students.AsNoTracking();
        
        studentQuery = ApplyFilters(studentQuery, query);

        studentQuery = ApplySearchTerm(studentQuery, query);

        studentQuery = ApplySorting(studentQuery, query.SortColumn, query.SortDirection);


        int count = await studentQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);
        studentQuery = studentQuery.ApplyPagination(page, pageSize);


        var items = await studentQuery.
            Select(StudentListItemDto.Projection)
            .ToListAsync(ct);

        return new PaginatedList<StudentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<Student> ApplySearchTerm(IQueryable<Student> query, GetStudentsQuery searchQuery)
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

    private IQueryable<Student> ApplyFilters(IQueryable<Student> query, GetStudentsQuery searchQuery)
    {
        if(searchQuery.States is not null &&
            searchQuery.States.Count != 0)
                query = query.Where(s => searchQuery.States.Contains(s.State));

        if (searchQuery.EmailConfirmed.HasValue)
            query = studentQueryService.GetStudentsWithEmailStatus(query, searchQuery.EmailConfirmed.Value);

        return query;
    }

    private static IQueryable<Student> ApplySorting(IQueryable<Student> query, string? sortColumn, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = "createdat";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = "desc";

        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(s => s.CreatedAtUtc) : query.OrderBy(s => s.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(s => s.LastModifiedUtc) : query.OrderBy(s => s.LastModifiedUtc),
            "name" => isDescending ? query.OrderByDescending(s => s.Name.Value) : query.OrderBy(s => s.Name.Value),
            "state" => isDescending ? query.OrderByDescending(s => s.State) : query.OrderBy(s => s.State),
            "joindate" => isDescending ? query.OrderByDescending(s => s.JoinDateUtc) : query.OrderBy(s => s.JoinDateUtc),
            _ => query.OrderByDescending(wo => wo.CreatedAtUtc) // Default sorting
        };
    }
}
