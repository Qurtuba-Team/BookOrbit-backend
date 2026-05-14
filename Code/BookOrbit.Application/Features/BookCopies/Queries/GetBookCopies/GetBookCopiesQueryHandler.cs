namespace BookOrbit.Application.Features.BookCopies.Queries.GetBookCopies;
public class GetBookCopiesQueryHandler(
    IAppDbContext context,
    IRouteService routeService) : BasePagedQueryHandler<BookCopy, BookCopyListItemDto, GetBookCopiesQuery>
{
    protected override Dictionary<string, Func<IQueryable<BookCopy>, bool, IOrderedQueryable<BookCopy>>> SortMappings
        => new()
        {
            ["createdat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.CreatedAtUtc)
                    : query.OrderBy(b => b.CreatedAtUtc),
            ["updatedat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.LastModifiedUtc)
                    : query.OrderBy(b => b.LastModifiedUtc),
            ["booktitle"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.Book!.Title.Value)
                    : query.OrderBy(b => b.Book!.Title.Value),
            ["ownername"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(b => b.Owner!.Name.Value)
                    : query.OrderBy(b => b.Owner!.Name.Value)
        };

    protected override IQueryable<BookCopy> GetBaseQuery()
    {
        return context.BookCopies.AsNoTracking();
    }

    protected override IQueryable<BookCopy> ApplySearch(IQueryable<BookCopy> query, GetBookCopiesQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;//no need for filters

        var normalizedTitle = BookTitle.Normalize(searchQuery.SearchTerm);
        var normalizedStudentName = StudentName.Normalize(searchQuery.SearchTerm);

        query = query.Where(b =>
            b.Book!.Title.Value.Contains(normalizedTitle) ||
            b.Owner!.Name.Value.Contains(normalizedStudentName));

        return query;
    }

    protected override IQueryable<BookCopy> ApplyFilters(IQueryable<BookCopy> query, GetBookCopiesQuery searchQuery)
    {
        if (searchQuery.OwnerId is not null)
            query = query.Where(b => b.OwnerId == searchQuery.OwnerId);

        if (searchQuery.BookId is not null)
            query = query.Where(b => b.BookId == searchQuery.BookId);

        if (searchQuery.States is not null &&
            searchQuery.States.Count != 0)
            query = query.Where(b => searchQuery.States.Contains(b.State));

        if (searchQuery.Conditions is not null && searchQuery.Conditions.Count != 0)
            query = query.Where(b => searchQuery.Conditions.Contains(b.Condition));

        return query;
    }

    protected override IQueryable<BookCopyListItemDto> ProjectToDto(IQueryable<BookCopy> query)
    {
        string baseUrl = routeService.GetBookCoverImageRoute();

        return query.Select(b => new BookCopyListItemDto(
            b.Id,
            b.BookId,
            b.OwnerId,
            b.Condition,
            b.State,
            b.Owner!.Name.Value,
            b.Book!.Title.Value,
            context.LendingListRecords.Any(l =>
                l.BookCopyId == b.Id &&
                (l.State == LendingListRecordState.Available ||
                 l.State == LendingListRecordState.Reserved ||
                 l.State == LendingListRecordState.Borrowed)),
            baseUrl + "/" + b.Book!.CoverImageFileName));
    }
}
