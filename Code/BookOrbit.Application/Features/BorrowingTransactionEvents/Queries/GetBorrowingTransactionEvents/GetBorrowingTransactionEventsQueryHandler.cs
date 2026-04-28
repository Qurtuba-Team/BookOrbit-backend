using BookOrbit.Application.Features.BorrowingTransactionEvents.Dtos;
using BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;

namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEvents;

public class GetBorrowingTransactionEventsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetBorrowingTransactionEventsQuery, Result<PaginatedList<BorrowingTransactionEventListItemDto>>>
{
    public async Task<Result<PaginatedList<BorrowingTransactionEventListItemDto>>> Handle(GetBorrowingTransactionEventsQuery query, CancellationToken ct)
    {
        var eventQuery = context.BorrowingTransactionEvents.AsNoTracking();

        eventQuery = ApplyFilters(eventQuery, query);
        eventQuery = ApplySearchTerm(eventQuery, query);
        eventQuery = ApplySorting(eventQuery, query.SortColumn, query.SortDirection);

        int count = await eventQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        var items = await eventQuery
            .ApplyPagination(page, pageSize)
            .Select(bte => BorrowingTransactionEventListItemDto.FromEntity(bte))
            .ToListAsync(ct);

        return new PaginatedList<BorrowingTransactionEventListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<BorrowingTransactionEvent> ApplyFilters(IQueryable<BorrowingTransactionEvent> query, GetBorrowingTransactionEventsQuery searchQuery)
    {
        if (searchQuery.BorrowingTransactionId is not null)
            query = query.Where(bte => bte.BorrowingTransactionId == searchQuery.BorrowingTransactionId);

        if (searchQuery.States is not null && searchQuery.States.Count != 0)
            query = query.Where(bte => searchQuery.States.Contains(bte.State));

        return query;
    }

    private static IQueryable<BorrowingTransactionEvent> ApplySearchTerm(
        IQueryable<BorrowingTransactionEvent> query,
        GetBorrowingTransactionEventsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        if (!Guid.TryParse(searchQuery.SearchTerm, out var parsedId))
            return query;

        return query.Where(bte => bte.Id == parsedId || bte.BorrowingTransactionId == parsedId);
    }

    private static IQueryable<BorrowingTransactionEvent> ApplySorting(
        IQueryable<BorrowingTransactionEvent> query,
        string? sortColumn,
        string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = "createdat";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = "desc";

        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(bte => bte.CreatedAtUtc) : query.OrderBy(bte => bte.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(bte => bte.LastModifiedUtc) : query.OrderBy(bte => bte.LastModifiedUtc),
            "state" => isDescending ? query.OrderByDescending(bte => bte.State) : query.OrderBy(bte => bte.State),
            _ => query.OrderByDescending(bte => bte.CreatedAtUtc)
        };
    }
}
