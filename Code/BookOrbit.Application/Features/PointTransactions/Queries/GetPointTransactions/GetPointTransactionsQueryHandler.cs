using BookOrbit.Application.Features.PointTransactions.Dtos;
using BookOrbit.Domain.PointTransactions;

namespace BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactions;

public class GetPointTransactionsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPointTransactionsQuery, Result<PaginatedList<PointTransactionListItemDto>>>
{
    public async Task<Result<PaginatedList<PointTransactionListItemDto>>> Handle(GetPointTransactionsQuery query, CancellationToken ct)
    {
        var transactionQuery = context.PointTransactions.AsNoTracking();

        transactionQuery = ApplyFilters(transactionQuery, query);

        var queryWithStudent = transactionQuery.Join(
            context.Students.AsNoTracking(),
            transaction => transaction.StudentId,
            student => student.Id,
            (transaction, student) => new PointTransactionWithStudentNameDto
            {
                Id = transaction.Id,
                StudentId = transaction.StudentId,
                StudentName = student.Name.Value,
                BorrowingReviewId = transaction.BorrowingReviewId,
                Points = transaction.Points,
                Reason = transaction.Reason,
                CreatedAtUtc = transaction.CreatedAtUtc,
                LastModifiedUtc = transaction.LastModifiedUtc
            });

        queryWithStudent = ApplySearchTerm(queryWithStudent, query);
        queryWithStudent = ApplySorting(queryWithStudent, query.SortColumn, query.SortDirection);

        int count = await queryWithStudent.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        var items = await queryWithStudent
            .ApplyPagination(page, pageSize)
            .ToListAsync(ct);

        var listItems = items
            .Select(PointTransactionListItemDto.FromIntermediate)
            .ToList();

        return new PaginatedList<PointTransactionListItemDto>
        {
            Items = listItems,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<PointTransaction> ApplyFilters(IQueryable<PointTransaction> query, GetPointTransactionsQuery searchQuery)
    {
        if (searchQuery.StudentId is not null)
            query = query.Where(pt => pt.StudentId == searchQuery.StudentId);

        if (searchQuery.BorrowingReviewId is not null)
            query = query.Where(pt => pt.BorrowingReviewId == searchQuery.BorrowingReviewId);

        if (searchQuery.Reasons is not null && searchQuery.Reasons.Count != 0)
            query = query.Where(pt => searchQuery.Reasons.Contains(pt.Reason));

        return query;
    }

    private static IQueryable<PointTransactionWithStudentNameDto> ApplySearchTerm(
        IQueryable<PointTransactionWithStudentNameDto> query,
        GetPointTransactionsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        var normalizedName = StudentName.Normalize(searchQuery.SearchTerm);

        return query.Where(pt => pt.StudentName.Contains(normalizedName));
    }

    private static IQueryable<PointTransactionWithStudentNameDto> ApplySorting(
        IQueryable<PointTransactionWithStudentNameDto> query,
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
            "createdat" => isDescending ? query.OrderByDescending(pt => pt.CreatedAtUtc) : query.OrderBy(pt => pt.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(pt => pt.LastModifiedUtc) : query.OrderBy(pt => pt.LastModifiedUtc),
            "points" => isDescending ? query.OrderByDescending(pt => pt.Points) : query.OrderBy(pt => pt.Points),
            "reason" => isDescending ? query.OrderByDescending(pt => pt.Reason) : query.OrderBy(pt => pt.Reason),
            "studentname" => isDescending ? query.OrderByDescending(pt => pt.StudentName) : query.OrderBy(pt => pt.StudentName),
            _ => query.OrderByDescending(pt => pt.CreatedAtUtc)
        };
    }
}
