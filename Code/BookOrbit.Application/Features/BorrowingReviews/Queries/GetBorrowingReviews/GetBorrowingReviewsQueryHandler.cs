
namespace BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviews;

public class GetBorrowingReviewsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetBorrowingReviewsQuery, Result<PaginatedList<BorrowingReviewListItemDto>>>
{
    public async Task<Result<PaginatedList<BorrowingReviewListItemDto>>> Handle(GetBorrowingReviewsQuery query, CancellationToken ct)
    {
        var reviewQuery = context.BorrowingReviews.AsNoTracking();

        reviewQuery = ApplyFilters(reviewQuery, query);
        reviewQuery = ApplySearchTerm(reviewQuery, query);
        reviewQuery = ApplySorting(reviewQuery, query.SortColumn, query.SortDirection);

        int count = await reviewQuery.CountAsync(ct);

        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);

        var items = await reviewQuery
            .ApplyPagination(page, pageSize)
            .Select(br => BorrowingReviewListItemDto.FromEntity(br))
            .ToListAsync(ct);

        return new PaginatedList<BorrowingReviewListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }

    private static IQueryable<BorrowingReview> ApplyFilters(IQueryable<BorrowingReview> query, GetBorrowingReviewsQuery searchQuery)
    {
        if (searchQuery.ReviewerStudentId is not null)
            query = query.Where(br => br.ReviewerStudentId == searchQuery.ReviewerStudentId);

        if (searchQuery.ReviewedStudentId is not null)
            query = query.Where(br => br.ReviewedStudentId == searchQuery.ReviewedStudentId);

        if (searchQuery.BorrowingTransactionId is not null)
            query = query.Where(br => br.BorrowingTransactionId == searchQuery.BorrowingTransactionId);

        return query;
    }

    private static IQueryable<BorrowingReview> ApplySearchTerm(IQueryable<BorrowingReview> query, GetBorrowingReviewsQuery searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            return query;

        if (Guid.TryParse(searchQuery.SearchTerm, out var parsedId))
        {
            return query.Where(br =>
                br.Id == parsedId ||
                br.BorrowingTransactionId == parsedId ||
                br.ReviewerStudentId == parsedId ||
                br.ReviewedStudentId == parsedId);
        }

        return query.Where(br => br.Description != null && br.Description.Contains(searchQuery.SearchTerm));
    }

    private static IQueryable<BorrowingReview> ApplySorting(
        IQueryable<BorrowingReview> query,
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
            "createdat" => isDescending ? query.OrderByDescending(br => br.CreatedAtUtc) : query.OrderBy(br => br.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(br => br.LastModifiedUtc) : query.OrderBy(br => br.LastModifiedUtc),
            "rating" => isDescending ? query.OrderByDescending(br => br.Rating.Value) : query.OrderBy(br => br.Rating.Value),
            _ => query.OrderByDescending(br => br.CreatedAtUtc)
        };
    }
}
