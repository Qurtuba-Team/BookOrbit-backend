namespace BookOrbit.Application.Features.BorrowingReviews.EventHandlers;
public class BorrowingReviewCreatedEventHandler(
    IAppDbContext context,
    ILogger<BorrowingReviewCreatedEventHandler> logger) : INotificationHandler<BorrowingReviewCreatedEvent>
{
    public async Task Handle(BorrowingReviewCreatedEvent notification, CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == notification.ReviewedStudentId, cancellationToken);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found to process borrowing review points.", notification.ReviewedStudentId);
            return;
        }

        int pointsValue = 0;
        bool isReward = false;
        PointTransactionReason reason = PointTransactionReason.GoodReview;

        switch (notification.RatingValue)
        {
            case 1:
                pointsValue = Math.Abs(Point.OneStarReviewPenalty);
                isReward = false;
                reason = PointTransactionReason.BadReview;
                break;
            case 2:
                pointsValue = Math.Abs(Point.TwoStarsReviewPenalty);
                isReward = false;
                reason = PointTransactionReason.BadReview;
                break;
            case 3:
                pointsValue = Math.Abs(Point.ThreeStarsReviewReward);
                isReward = true;
                reason = PointTransactionReason.GoodReview;
                break;
            case 4:
                pointsValue = Math.Abs(Point.FourStarsReviewReward);
                isReward = true;
                reason = PointTransactionReason.GoodReview;
                break;
            case 5:
                pointsValue = Math.Abs(Point.FiveStarsReviewReward);
                isReward = true;
                reason = PointTransactionReason.GoodReview;
                break;
            default:
                logger.LogWarning("Invalid rating value {RatingValue} for borrowing review points.", notification.RatingValue);
                return;
        }

        if (pointsValue > 0) // if its 0 ignore
        {
            var pointResult = Point.Create(pointsValue);
            if (pointResult.IsFailure)
            {
                logger.LogWarning("Failed to create point object. Errors: {Errors}", pointResult.Errors);
                return;
            }

            if (isReward)
            {
                student.AddPoints(pointResult.Value, reason, notification.BorrowingReviewId);
            }
            else
            {
                student.DeductPoints(pointResult.Value, reason, notification.BorrowingReviewId);
            }
        }

        logger.LogInformation("Successfully processed points for borrowing review {BorrowingReviewId}. IsReward: {IsReward}, Points: {Points}", 
            notification.BorrowingReviewId, isReward, pointsValue);
    }
}
