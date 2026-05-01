namespace BookOrbit.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetNotificationsQuery, Result<PaginatedList<NotificationListItemDto>>>
{
    public async Task<Result<PaginatedList<NotificationListItemDto>>> Handle(GetNotificationsQuery query, CancellationToken ct)
    {
        var notificationsQuery = context.Notification.AsNoTracking()
            .Where(n => n.StudentId == query.StudentId);

        notificationsQuery = ApplyFilters(notificationsQuery, query);
        notificationsQuery = ApplySearchTerm(notificationsQuery, query);
        notificationsQuery = ApplySorting(notificationsQuery, query.SortColumn, query.SortDirection);

        int count = await notificationsQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        var items = await notificationsQuery
            .ApplyPagination(page, pageSize)
            .Select(NotificationListItemDto.Projection)
            .ToListAsync(ct);

        return new PaginatedList<NotificationListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<Notification> ApplyFilters(IQueryable<Notification> query, GetNotificationsQuery searchQuery)
    {
        if (searchQuery.IsRead is not null)
            query = query.Where(n => n.IsRead == searchQuery.IsRead);

        if (searchQuery.Types is not null && searchQuery.Types.Count != 0)
            query = query.Where(n => searchQuery.Types.Contains(n.Type));

        return query;
    }

    private static IQueryable<Notification> ApplySearchTerm(IQueryable<Notification> query, GetNotificationsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        var searchTerm = searchQuery.SearchTerm.Trim();

        return query.Where(n =>
            n.Title.Contains(searchTerm) ||
            n.Message.Contains(searchTerm));
    }

    private static IQueryable<Notification> ApplySorting(IQueryable<Notification> query, string? sortColumn, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = "createdat";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = "desc";

        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(n => n.CreatedAtUtc) : query.OrderBy(n => n.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(n => n.LastModifiedUtc) : query.OrderBy(n => n.LastModifiedUtc),
            "title" => isDescending ? query.OrderByDescending(n => n.Title) : query.OrderBy(n => n.Title),
            "type" => isDescending ? query.OrderByDescending(n => n.Type) : query.OrderBy(n => n.Type),
            "isread" => isDescending ? query.OrderByDescending(n => n.IsRead) : query.OrderBy(n => n.IsRead),
            _ => query.OrderByDescending(n => n.CreatedAtUtc)
        };
    }
}
