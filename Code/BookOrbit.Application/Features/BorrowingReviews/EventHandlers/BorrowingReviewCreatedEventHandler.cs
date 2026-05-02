using BookOrbit.Application.Common.Interfaces.SystemNotificationService;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.DomainEvents;

namespace BookOrbit.Application.Features.BorrowingReviews.EventHandlers;
public class BorrowingReviewCreatedEventHandler(
    IAppDbContext context,
    ILogger<BorrowingReviewCreatedEventHandler> logger,
    ISystemNotificationService systemNotificationService) : INotificationHandler<BorrowingReviewCreatedEvent>
{
    private async Task Notify(Guid ReviewdStudentId, int ratingValue,bool isReward ,CancellationToken ct)
    {
        string title = $"You have been rated {ratingValue} stars";
        string message = $"You have been rated  {ratingValue}  stars";

        await systemNotificationService.SendNotificationAsync(ReviewdStudentId, title, message, 
            isReward? NotificationType.Good
            : NotificationType.Bad, ct);
    }
    private Task ManageStudentsPoints((int pointsValue, bool isReward, PointTransactionReason reason) pointsInfo, BorrowingReviewCreatedEvent notification, Student student)
    {

        if (pointsInfo.pointsValue > 0) // if its 0 ignore
        {
            var pointResult = Point.Create(pointsInfo.pointsValue);
            if (pointResult.IsFailure)
            {
                logger.LogWarning("Failed to create point object. Errors: {Errors}", pointResult.Errors);
                return Task.CompletedTask;
            }

            if (pointsInfo.isReward)
            {
                student.AddPoints(pointResult.Value, pointsInfo.reason, notification.BorrowingReviewId);
            }
            else
            {
                student.DeductPoints(pointResult.Value, pointsInfo.reason, notification.BorrowingReviewId);
            }
        }

        logger.LogInformation("Successfully processed points for borrowing review {BorrowingReviewId}. IsReward: {IsReward}, Points: {Points}",
            notification.BorrowingReviewId, pointsInfo.isReward, pointsInfo.pointsValue);

        return Task.CompletedTask;
    }
    private (int pointsValue, bool isReward, PointTransactionReason reason) Calculate(BorrowingReviewCreatedEvent notification)
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
                break;
        }

        return (pointsValue, isReward, reason);
    }
    public async Task Handle(BorrowingReviewCreatedEvent notification ,CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == notification.ReviewedStudentId, cancellationToken);

        if (student is null)
        {
            logger.LogWarning("Student {StudentId} not found to process borrowing review points.", notification.ReviewedStudentId);
            return;
        }

        var pointsInfo = Calculate(notification);
        await ManageStudentsPoints(pointsInfo, notification, student);
        await Notify(notification.ReviewedStudentId, notification.RatingValue, pointsInfo.isReward, cancellationToken);
    }
}