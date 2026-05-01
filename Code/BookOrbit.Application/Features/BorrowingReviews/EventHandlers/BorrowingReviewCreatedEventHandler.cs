namespace BookOrbit.Application.Features.BorrowingReviews.EventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Domain.Students;
using BookOrbit.Domain.PointTransactions.ValueObjects;
using BookOrbit.Domain.PointTransactions.Enums;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.DomainEvents;
using BookOrbit.Application.Features.Students;
using Microsoft.EntityFrameworkCore;

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
                pointsValue = Point.ThreeStarsReviewReward;
                isReward = true;
                reason = PointTransactionReason.GoodReview;
                break;
            case 4:
                pointsValue = Point.FourStarsReviewReward;
                isReward = true;
                reason = PointTransactionReason.GoodReview;
                break;
            case 5:
                pointsValue = Point.FiveStarsReviewReward;
                isReward = true;
                reason = PointTransactionReason.GoodReview;
                break;
            default:
                logger.LogWarning("Invalid rating value {RatingValue} for borrowing review points.", notification.RatingValue);
                return;
        }

        if (pointsValue > 0)
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

        // Three stars might be 0, so if pointsValue == 0, we don't necessarily need to add/deduct 0 points,
        // but if we want to record the transaction, maybe we should. Wait, if pointsValue == 0, Point.Create(0) 
        // will return success because 0 is valid. 
        if (pointsValue == 0)
        {
            // Just return or log if we don't want to create a 0 point transaction
            logger.LogInformation("No points added or deducted for 3-star rating. ReviewId: {ReviewId}", notification.BorrowingReviewId);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully processed points for borrowing review {BorrowingReviewId}. IsReward: {IsReward}, Points: {Points}", 
            notification.BorrowingReviewId, isReward, pointsValue);
    }
}
