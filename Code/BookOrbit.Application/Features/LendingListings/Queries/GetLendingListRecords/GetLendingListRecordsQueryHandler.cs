namespace BookOrbit.Application.Features.LendingListings.Queries.GetLendingListRecords;
public class GetLendingListRecordsQueryHandler(
    IAppDbContext context) : BasePagedQueryHandler<LendingListRecord, LendingListRecordListItemDto, GetLendingListRecordsQuery>
{
    protected override Dictionary<string, Func<IQueryable<LendingListRecord>, bool, IOrderedQueryable<LendingListRecord>>> SortMappings
        => new()
        {
            ["createdat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(lr => lr.CreatedAtUtc)
                    : query.OrderBy(lr => lr.CreatedAtUtc),
            ["updatedat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(lr => lr.LastModifiedUtc)
                    : query.OrderBy(lr => lr.LastModifiedUtc),
            ["cost"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(lr => lr.Cost.Value)
                    : query.OrderBy(lr => lr.Cost.Value),
            ["borrowingduration"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(lr => lr.BorrowingDurationInDays)
                    : query.OrderBy(lr => lr.BorrowingDurationInDays),
            ["expirationdate"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(lr => lr.ExpirationDateUtc)
                    : query.OrderBy(lr => lr.ExpirationDateUtc),
            ["state"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(lr => lr.State)
                    : query.OrderBy(lr => lr.State)
        };

    protected override IQueryable<LendingListRecord> GetBaseQuery()
    {
        return context.LendingListRecords.AsNoTracking();
    }

    protected override IQueryable<LendingListRecord> ApplyFilters(IQueryable<LendingListRecord> query, GetLendingListRecordsQuery searchQuery)
    {
        if (searchQuery.BookCopyId is not null)
            query = query.Where(lr => lr.BookCopyId == searchQuery.BookCopyId);

        if (searchQuery.BookId is not null)
            query = query.Where(lr => lr.BookCopy!.BookId == searchQuery.BookId);

        if(searchQuery.OwnerId is not null)
            query = query.Where(lr => lr.BookCopy!.OwnerId == searchQuery.OwnerId);

        if (searchQuery.States is not null &&
            searchQuery.States.Count != 0)
            query = query.Where(lr => searchQuery.States.Contains(lr.State));

        return query;
    }

    protected override IQueryable<LendingListRecord> ApplySearch(IQueryable<LendingListRecord> query, GetLendingListRecordsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;
        
        var normalizedTitle = BookTitle.Normalize(searchQuery.SearchTerm);

        query = query.Where(lr => lr.BookCopy!.Book!.Title.Value.Contains(normalizedTitle));

        return query;
    }

    protected override IQueryable<LendingListRecordListItemDto> ProjectToDto(IQueryable<LendingListRecord> query)
    {
        return query.Select(LendingListRecordListItemDto.Projection);
    }
}
