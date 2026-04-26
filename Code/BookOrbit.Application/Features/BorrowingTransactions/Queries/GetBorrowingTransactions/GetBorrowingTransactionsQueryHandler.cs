using BookOrbit.Application.Features.BorrowingTransactions.Dtos;

namespace BookOrbit.Application.Features.BorrowingTransactions.Queries.GetBorrowingTransactions;
public class GetBorrowingTransactionsQueryHandler(
    IAppDbContext context) : IRequestHandler<GetBorrowingTransactionsQuery, Result<PaginatedList<BorrowingTransactionListItemDto>>>
{
    public async Task<Result<PaginatedList<BorrowingTransactionListItemDto>>> Handle(GetBorrowingTransactionsQuery query, CancellationToken ct)
    {
        var transactionQuery = context.BorrowingTransactions.AsNoTracking();

        transactionQuery = ApplyFilters(transactionQuery, query);
        transactionQuery = ApplySearchTerm(transactionQuery, query);
        transactionQuery = ApplySorting(transactionQuery, query.SortColumn, query.SortDirection);

        int count = await transactionQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        transactionQuery = transactionQuery.ApplyPagination(page, pageSize);

        var items = await transactionQuery
            .Select(BorrowingTransactionListItemDto.Projection)
            .ToListAsync(ct);

        return new PaginatedList<BorrowingTransactionListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<BorrowingTransaction> ApplyFilters(IQueryable<BorrowingTransaction> query, GetBorrowingTransactionsQuery searchQuery)
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

    private static IQueryable<BorrowingTransaction> ApplySearchTerm(IQueryable<BorrowingTransaction> query, GetBorrowingTransactionsQuery searchQuery)
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

    private static IQueryable<BorrowingTransaction> ApplySorting(IQueryable<BorrowingTransaction> query, string? sortColumn, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = "createdat";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = "desc";

        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(bt => bt.CreatedAtUtc) : query.OrderBy(bt => bt.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(bt => bt.LastModifiedUtc) : query.OrderBy(bt => bt.LastModifiedUtc),
            "expectedreturndate" => isDescending ? query.OrderByDescending(bt => bt.ExpectedReturnDate) : query.OrderBy(bt => bt.ExpectedReturnDate),
            "actualreturndate" => isDescending ? query.OrderByDescending(bt => bt.ActualReturnDate) : query.OrderBy(bt => bt.ActualReturnDate),
            "state" => isDescending ? query.OrderByDescending(bt => bt.State) : query.OrderBy(bt => bt.State),
            "borrowername" => isDescending ? query.OrderByDescending(bt => bt.BorrowerStudent!.Name!.Value) : query.OrderBy(bt => bt.BorrowerStudent!.Name!.Value),
            "lendername" => isDescending ? query.OrderByDescending(bt => bt.LenderStudent!.Name!.Value) : query.OrderBy(bt => bt.LenderStudent!.Name!.Value),
            "booktitle" => isDescending ? query.OrderByDescending(bt => bt.BookCopy!.Book!.Title!.Value) : query.OrderBy(bt => bt.BookCopy!.Book!.Title!.Value),
            _ => query.OrderByDescending(bt => bt.CreatedAtUtc)
        };
    }
}
