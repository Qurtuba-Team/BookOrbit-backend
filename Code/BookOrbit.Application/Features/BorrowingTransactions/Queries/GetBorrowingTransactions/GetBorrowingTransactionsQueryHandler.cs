
namespace BookOrbit.Application.Features.BorrowingTransactions.Queries.GetBorrowingTransactions;
public class GetBorrowingTransactionsQueryHandler(
    IAppDbContext context) : BasePagedQueryHandler<BorrowingTransaction, BorrowingTransactionListItemDto, GetBorrowingTransactionsQuery>
{
    protected override Dictionary<string, Func<IQueryable<BorrowingTransaction>, bool, IOrderedQueryable<BorrowingTransaction>>> SortMappings
        => new()
        {
            ["createdat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bt => bt.CreatedAtUtc)
                    : query.OrderBy(bt => bt.CreatedAtUtc),
            ["updatedat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bt => bt.LastModifiedUtc)
                    : query.OrderBy(bt => bt.LastModifiedUtc),
            ["expectedreturndate"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bt => bt.ExpectedReturnDate)
                    : query.OrderBy(bt => bt.ExpectedReturnDate),
            ["actualreturndate"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bt => bt.ActualReturnDate)
                    : query.OrderBy(bt => bt.ActualReturnDate),
            ["state"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bt => bt.State)
                    : query.OrderBy(bt => bt.State),
            ["borrowername"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bt => bt.BorrowerStudent!.Name!.Value)
                    : query.OrderBy(bt => bt.BorrowerStudent!.Name!.Value),
            ["lendername"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bt => bt.LenderStudent!.Name!.Value)
                    : query.OrderBy(bt => bt.LenderStudent!.Name!.Value),
            ["booktitle"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(bt => bt.BookCopy!.Book!.Title!.Value)
                    : query.OrderBy(bt => bt.BookCopy!.Book!.Title!.Value)
        };

    protected override IQueryable<BorrowingTransaction> GetBaseQuery()
    {
        return context.BorrowingTransactions.AsNoTracking();
    }

    protected override IQueryable<BorrowingTransaction> ApplyFilters(IQueryable<BorrowingTransaction> query, GetBorrowingTransactionsQuery searchQuery)
    {
        if (searchQuery.BorrowerStudentId is not null)
            query = query.Where(bt => bt.BorrowerStudentId == searchQuery.BorrowerStudentId);

        if (searchQuery.LenderStudentId is not null)
            query = query.Where(bt => bt.LenderStudentId == searchQuery.LenderStudentId);

        if (searchQuery.BookCopyId is not null)
            query = query.Where(bt => bt.BookCopyId == searchQuery.BookCopyId);

        if (searchQuery.BorrowingRequestId is not null)
            query = query.Where(bt => bt.BorrowingRequestId == searchQuery.BorrowingRequestId);

        if (searchQuery.States is not null && searchQuery.States.Count != 0)
            query = query.Where(bt => searchQuery.States.Contains(bt.State));

        return query;
    }

    protected override IQueryable<BorrowingTransaction> ApplySearch(IQueryable<BorrowingTransaction> query, GetBorrowingTransactionsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        var normalizedStudentName = StudentName.Normalize(searchQuery.SearchTerm);
        var normalizedBookTitle = BookTitle.Normalize(searchQuery.SearchTerm);

        return query.Where(bt =>
            bt.BorrowerStudent!.Name!.Value.Contains(normalizedStudentName) ||
            bt.LenderStudent!.Name!.Value.Contains(normalizedStudentName) ||
            bt.BookCopy!.Book!.Title!.Value.Contains(normalizedBookTitle));
    }

    protected override IQueryable<BorrowingTransactionListItemDto> ProjectToDto(IQueryable<BorrowingTransaction> query)
    {
        return query.Select(BorrowingTransactionListItemDto.Projection);
    }
}
