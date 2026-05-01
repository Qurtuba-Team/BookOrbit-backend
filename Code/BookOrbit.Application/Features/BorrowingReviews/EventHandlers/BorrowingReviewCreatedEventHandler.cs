using BookOrbit.Application.Common.Interfaces.SystemNotificationService;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.DomainEvents;

namespace BookOrbit.Application.Features.BorrowingReviews.EventHandlers;
public class BorrowingReviewCreatedEventHandler(
    IAppDbContext context,
    ILogger<BorrowingReviewCreatedEventHandler> logger,
    ISystemNotificationService systemNotificationService) : INotificationHandler<BorrowingReviewCreatedEvent>
{
    private async Task Notify(Guid ReviewdStudentId, int ratingValue, CancellationToken ct)
    {
        string title = $"You have been rated {ratingValue} stars";
        string message = $"You have been rated  {ratingValue}  stars";

        await systemNotificationService.SendNotificationAsync(ReviewdStudentId, title, message, NotificationType.Normal, ct);
    }
    private Task ManageStudentsPoints(BorrowingReviewCreatedEvent notification, Student student)
    {
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
                return Task.CompletedTask;
        }

        if (pointsValue > 0) // if its 0 ignore
        {
            var pointResult = Point.Create(pointsValue);
            if (pointResult.IsFailure)
            {
                logger.LogWarning("Failed to create point object. Errors: {Errors}", pointResult.Errors);
                return Task.CompletedTask;
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

        return Task.CompletedTask;
    }
    public async Task Handle(BorrowingReviewCreatedEvent notification, CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == notification.ReviewedStudentId, cancellationToken);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found to process borrowing review points.", notification.ReviewedStudentId);
            return;
        }

        await ManageStudentsPoints(notification, student);
        await Notify(notification.ReviewedStudentId, notification.RatingValue, cancellationToken);
    }
}
