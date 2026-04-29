using BookOrbit.Application.Features.BorrowingReviews.Dtos;
using BookOrbit.Application.Features.BorrowingTransactions;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;

namespace BookOrbit.Application.Features.BorrowingReviews.Commands.CreateBorrowingReview;

public class CreateBorrowingReviewCommandHandler(
    ILogger<CreateBorrowingReviewCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<CreateBorrowingReviewCommand, Result<BorrowingReviewDto>>
{
    public async Task<Result<BorrowingReviewDto>> Handle(CreateBorrowingReviewCommand command, CancellationToken ct)
    {
        var reviewerExists = await context.Students
            .AsNoTracking()
            .AnyAsync(s => s.Id == command.ReviewerStudentId, ct);

        if (!reviewerExists)
        {
            logger.LogWarning("Reviewer student not found. ReviewerStudentId: {ReviewerStudentId}", command.ReviewerStudentId);
            return StudentApplicationErrors.NotFoundById;
        }

        var reviewedExists = await context.Students
            .AsNoTracking()
            .AnyAsync(s => s.Id == command.ReviewedStudentId, ct);

        if (!reviewedExists)
        {
            logger.LogWarning("Reviewed student not found. ReviewedStudentId: {ReviewedStudentId}", command.ReviewedStudentId);
            return StudentApplicationErrors.NotFoundById;
        }

        var transactionState = await context.BorrowingTransactions
            .AsNoTracking()
            .Select(bt => new
            {
                bt.Id,
                bt.State
            })
            .FirstOrDefaultAsync(bt => bt.Id == command.BorrowingTransactionId, ct);

        if (transactionState is null)
        {
            logger.LogWarning("Borrowing transaction not found. BorrowingTransactionId: {BorrowingTransactionId}", command.BorrowingTransactionId);
            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        if(transactionState is not BorrowingTransactionState.Returned)
        {
            logger.LogWarning("Borrowing transaction is not in a returned state. BorrowingTransactionId: {BorrowingTransactionId}", command.BorrowingTransactionId);
            return BorrowingTransactionApplicationErrors.InvalidState;
        }

        var reviewAlreadyExists = await context.BorrowingReviews
            .AsNoTracking()
            .AnyAsync(br => br.BorrowingTransactionId == command.BorrowingTransactionId, ct);

        if (reviewAlreadyExists)
        {
            logger.LogWarning("Borrowing review already exists for transaction. BorrowingTransactionId: {BorrowingTransactionId}", command.BorrowingTransactionId);
            return BorrowingReviewApplicationErrors.AlreadyExists;
        }

        var ratingResult = StartsRating.Create(command.Rating);

        if (ratingResult.IsFailure)
        {
            logger.LogWarning("Invalid rating for borrowing review. Errors: {Errors}", ratingResult.Errors);
            return ratingResult.Errors;
        }

        var reviewResult = BorrowingReview.Create(
            Guid.NewGuid(),
            command.ReviewerStudentId,
            command.ReviewedStudentId,
            command.BorrowingTransactionId,
            command.Description,
            ratingResult.Value);

        if (reviewResult.IsFailure)
        {
            logger.LogWarning("Failed to create borrowing review. Errors: {Errors}", reviewResult.Errors);
            return reviewResult.Errors;
        }

        context.BorrowingReviews.Add(reviewResult.Value);
        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync(BorrowingReviewCachingConstants.BorrowingReviewTag, ct);

        logger.LogInformation("Borrowing review {BorrowingReviewId} created.", reviewResult.Value.Id);

        return BorrowingReviewDto.FromEntity(reviewResult.Value);
    }
}
