
namespace BookOrbit.Application.Features.PointTransactions.Queries.GetPointTransactions;

public class GetPointTransactionsQueryHandler(IAppDbContext context)
    : BasePagedQueryHandler<PointTransaction, PointTransactionListItemDto, GetPointTransactionsQuery>
{
    protected override Dictionary<string, Func<IQueryable<PointTransaction>, bool, IOrderedQueryable<PointTransaction>>> SortMappings
        => new()
        {
            ["createdat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(pt => pt.CreatedAtUtc)
                    : query.OrderBy(pt => pt.CreatedAtUtc),
            ["updatedat"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(pt => pt.LastModifiedUtc)
                    : query.OrderBy(pt => pt.LastModifiedUtc),
            ["points"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(pt => pt.Points)
                    : query.OrderBy(pt => pt.Points),
            ["reason"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(pt => pt.Reason)
                    : query.OrderBy(pt => pt.Reason),
            ["studentname"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(pt => context.Students
                        .Where(s => s.Id == pt.StudentId)
                        .Select(s => s.Name.Value)
                        .FirstOrDefault())
                    : query.OrderBy(pt => context.Students
                        .Where(s => s.Id == pt.StudentId)
                        .Select(s => s.Name.Value)
                        .FirstOrDefault())
        };

    protected override IQueryable<PointTransaction> GetBaseQuery()
    {
        return context.PointTransactions.AsNoTracking();
    }

    protected override IQueryable<PointTransaction> ApplyFilters(IQueryable<PointTransaction> query, GetPointTransactionsQuery searchQuery)
    {
        if (searchQuery.StudentId is not null)
            query = query.Where(pt => pt.StudentId == searchQuery.StudentId);

        if (searchQuery.BorrowingReviewId is not null)
            query = query.Where(pt => pt.BorrowingReviewId == searchQuery.BorrowingReviewId);

        if (searchQuery.Reasons is not null && searchQuery.Reasons.Count != 0)
            query = query.Where(pt => searchQuery.Reasons.Contains(pt.Reason));

        return query;
    }

    protected override IQueryable<PointTransaction> ApplySearch(
        IQueryable<PointTransaction> query,
        GetPointTransactionsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        var normalizedName = StudentName.Normalize(searchQuery.SearchTerm);

        return query.Where(pt => context.Students
            .Any(s => s.Id == pt.StudentId && s.Name.Value.Contains(normalizedName)));
    }

    protected override IQueryable<PointTransactionListItemDto> ProjectToDto(IQueryable<PointTransaction> query)
    {
        return query.Join(
            context.Students.AsNoTracking(),
            transaction => transaction.StudentId,
            student => student.Id,
            (transaction, student) => new PointTransactionListItemDto(
                transaction.Id,
                transaction.StudentId,
                student.Name.Value,
                transaction.BorrowingReviewId,
                transaction.Points,
                transaction.Reason,
                transaction.Reason == PointTransactionReason.BadReview ||
                transaction.Reason == PointTransactionReason.Borrowing ||
                transaction.Reason == PointTransactionReason.NegativeAdjustment ||
                transaction.Reason == PointTransactionReason.Penalty
                    ? PointTransactionDirection.Deduct
                    : transaction.Reason == PointTransactionReason.GoodReview ||
                      transaction.Reason == PointTransactionReason.Returning ||
                      transaction.Reason == PointTransactionReason.PositiveAdjustment ||
                      transaction.Reason == PointTransactionReason.Reward ||
                      transaction.Reason == PointTransactionReason.BookBorrowedFrom
                        ? PointTransactionDirection.Add
                        : PointTransactionDirection.None,
                transaction.CreatedAtUtc));
    }
}
