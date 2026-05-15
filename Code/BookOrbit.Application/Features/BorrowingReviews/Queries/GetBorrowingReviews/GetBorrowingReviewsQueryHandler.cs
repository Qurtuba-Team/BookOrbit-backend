
namespace BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviews;

public class GetBorrowingReviewsQueryHandler(IAppDbContext context)
    : BasePagedQueryHandler<BorrowingReview, BorrowingReviewListItemDto, GetBorrowingReviewsQuery>
{
    protected override Dictionary<string, Func<IQueryable<BorrowingReview>, bool, IOrderedQueryable<BorrowingReview>>> SortMappings
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
            ["rating"] = (query, desc) =>
                desc
                    ? query.OrderByDescending(br => br.Rating.Value)
                    : query.OrderBy(br => br.Rating.Value)
        };

    protected override IQueryable<BorrowingReview> GetBaseQuery()
    {
        return context.BorrowingReviews.AsNoTracking();
    }

    protected override IQueryable<BorrowingReview> ApplyFilters(IQueryable<BorrowingReview> query, GetBorrowingReviewsQuery searchQuery)
    {
        if (searchQuery.ReviewerStudentId is not null)
            query = query.Where(br => br.ReviewerStudentId == searchQuery.ReviewerStudentId);

        if (searchQuery.ReviewedStudentId is not null)
            query = query.Where(br => br.ReviewedStudentId == searchQuery.ReviewedStudentId);

        if (searchQuery.BorrowingTransactionId is not null)
            query = query.Where(br => br.BorrowingTransactionId == searchQuery.BorrowingTransactionId);

        return query;
    }

    protected override IQueryable<BorrowingReview> ApplySearch(IQueryable<BorrowingReview> query, GetBorrowingReviewsQuery searchQuery)
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

    protected override IQueryable<BorrowingReviewListItemDto> ProjectToDto(IQueryable<BorrowingReview> query)
    {
        return query.Select(br => BorrowingReviewListItemDto.FromEntity(br));
    }
}
