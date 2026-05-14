
namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Queries.GetBorrowingTransactionEvents;

public class GetBorrowingTransactionEventsQueryHandler(IAppDbContext context)
    : BasePagedQueryHandler<BorrowingTransactionEvent, BorrowingTransactionEventListItemDto, GetBorrowingTransactionEventsQuery>
{
    protected override Dictionary<string, Func<IQueryable<BorrowingTransactionEvent>, bool, IOrderedQueryable<BorrowingTransactionEvent>>> SortMappings
        => new()
        {
            ["createdat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bte => bte.CreatedAtUtc)
                    : query.OrderBy(bte => bte.CreatedAtUtc),
            ["updatedat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bte => bte.LastModifiedUtc)
                    : query.OrderBy(bte => bte.LastModifiedUtc),
            ["state"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bte => bte.State)
                    : query.OrderBy(bte => bte.State)
        };

    protected override IQueryable<BorrowingTransactionEvent> GetBaseQuery()
    {
        return context.BorrowingTransactionEvents.AsNoTracking();
    }

    protected override IQueryable<BorrowingTransactionEvent> ApplyFilters(IQueryable<BorrowingTransactionEvent> query, GetBorrowingTransactionEventsQuery searchQuery)
    {
        if (searchQuery.BorrowingTransactionId is not null)
            query = query.Where(bte => bte.BorrowingTransactionId == searchQuery.BorrowingTransactionId);

        if (searchQuery.States is not null && searchQuery.States.Count != 0)
            query = query.Where(bte => searchQuery.States.Contains(bte.State));

        return query;
    }

    protected override IQueryable<BorrowingTransactionEvent> ApplySearch(
        IQueryable<BorrowingTransactionEvent> query,
        GetBorrowingTransactionEventsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        if (!Guid.TryParse(searchQuery.SearchTerm, out var parsedId))
            return query;

        return query.Where(bte => bte.Id == parsedId || bte.BorrowingTransactionId == parsedId);
    }

    protected override IQueryable<BorrowingTransactionEventListItemDto> ProjectToDto(IQueryable<BorrowingTransactionEvent> query)
    {
        return query.Select(bte => BorrowingTransactionEventListItemDto.FromEntity(bte));
    }
}
