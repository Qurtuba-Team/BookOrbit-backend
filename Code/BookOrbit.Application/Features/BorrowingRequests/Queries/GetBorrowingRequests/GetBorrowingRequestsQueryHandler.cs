namespace BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequests;
public class GetBorrowingRequestsQueryHandler(
    IAppDbContext context) : BasePagedQueryHandler<BorrowingRequest, BorrowingRequestListItemDto, GetBorrowingRequestsQuery>
{
    protected override Dictionary<string, Func<IQueryable<BorrowingRequest>, bool, IOrderedQueryable<BorrowingRequest>>> SortMappings
        => new()
        {
            ["createdat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(br => br.CreatedAtUtc)
                    : query.OrderBy(br => br.CreatedAtUtc),
            ["updatedat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(br => br.LastModifiedUtc)
                    : query.OrderBy(br => br.LastModifiedUtc),
            ["expirationdate"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(br => br.ExpirationDateUtc)
                    : query.OrderBy(br => br.ExpirationDateUtc),
            ["state"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(br => br.State)
                    : query.OrderBy(br => br.State),
            ["borrowername"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(br => br.BorrowingStudent!.Name.Value)
                    : query.OrderBy(br => br.BorrowingStudent!.Name.Value),
            ["booktitle"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(br => br.LendingRecord!.BookCopy!.Book!.Title.Value)
                    : query.OrderBy(br => br.LendingRecord!.BookCopy!.Book!.Title.Value)
        };

    protected override IQueryable<BorrowingRequest> GetBaseQuery()
    {
        return context.BorrowingRequests.AsNoTracking();
    }

    protected override IQueryable<BorrowingRequest> ApplyFilters(IQueryable<BorrowingRequest> query, GetBorrowingRequestsQuery searchQuery)
    {
        if (searchQuery.BorrowingStudentId is not null)
            query = query.Where(br => br.BorrowingStudentId == searchQuery.BorrowingStudentId);

        if (searchQuery.LendingRecordId is not null)
            query = query.Where(br => br.LendingRecordId == searchQuery.LendingRecordId);

        if (searchQuery.LendingStudentId is not null)
            query = query.Where(br => br.LendingRecord!.BookCopy!.OwnerId == searchQuery.LendingStudentId);

        if (searchQuery.States is not null && searchQuery.States.Count != 0)
            query = query.Where(br => searchQuery.States.Contains(br.State));

        return query;
    }

    protected override IQueryable<BorrowingRequest> ApplySearch(IQueryable<BorrowingRequest> query, GetBorrowingRequestsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        var normalizedStudentName = StudentName.Normalize(searchQuery.SearchTerm);
        var normalizedBookTitle = BookTitle.Normalize(searchQuery.SearchTerm);

        return query.Where(br =>
            br.BorrowingStudent!.Name.Value.Contains(normalizedStudentName) ||
            br.LendingRecord!.BookCopy!.Book!.Title.Value.Contains(normalizedBookTitle));
    }

    protected override IQueryable<BorrowingRequestListItemDto> ProjectToDto(IQueryable<BorrowingRequest> query)
    {
        return query.Select(BorrowingRequestListItemDto.Projection);
    }
}
