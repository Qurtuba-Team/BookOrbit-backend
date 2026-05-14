namespace BookOrbit.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler(IAppDbContext context)
    : BasePagedQueryHandler<Notification, NotificationListItemDto, GetNotificationsQuery>
{
    protected override Dictionary<string, Func<IQueryable<Notification>, bool, IOrderedQueryable<Notification>>> SortMappings
        => new()
        {
            ["createdat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(n => n.CreatedAtUtc)
                    : query.OrderBy(n => n.CreatedAtUtc),
            ["updatedat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(n => n.LastModifiedUtc)
                    : query.OrderBy(n => n.LastModifiedUtc),
            ["title"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(n => n.Title)
                    : query.OrderBy(n => n.Title),
            ["type"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(n => n.Type)
                    : query.OrderBy(n => n.Type),
            ["isread"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(n => n.IsRead)
                    : query.OrderBy(n => n.IsRead)
        };

    protected override IQueryable<Notification> GetBaseQuery()
    {
        return context.Notification.AsNoTracking();
    }

    protected override IQueryable<Notification> ApplyFilters(IQueryable<Notification> query, GetNotificationsQuery searchQuery)
    {
        query = query.Where(n => n.StudentId == searchQuery.StudentId);

        if (searchQuery.IsRead is not null)
            query = query.Where(n => n.IsRead == searchQuery.IsRead);

        if (searchQuery.Types is not null && searchQuery.Types.Count != 0)
            query = query.Where(n => searchQuery.Types.Contains(n.Type));

        return query;
    }

    protected override IQueryable<Notification> ApplySearch(IQueryable<Notification> query, GetNotificationsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        var searchTerm = searchQuery.SearchTerm.Trim();

        return query.Where(n =>
            n.Title.Contains(searchTerm) ||
            n.Message.Contains(searchTerm));
    }

    protected override IQueryable<NotificationListItemDto> ProjectToDto(IQueryable<Notification> query)
    {
        return query.Select(NotificationListItemDto.Projection);
    }
}
