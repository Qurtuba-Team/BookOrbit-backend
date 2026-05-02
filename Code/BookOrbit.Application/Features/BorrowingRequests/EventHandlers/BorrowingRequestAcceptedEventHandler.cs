using BookOrbit.Application.Common.Interfaces.SystemNotificationService;

namespace BookOrbit.Application.Features.BorrowingRequests.EventHandlers;
public class BorrowingRequestAcceptedEventHandler(
    IEmailService emailService,
    IAppDbContext context,
    ILogger<BorrowingRequestAcceptedEventHandler> logger,
    IEmailFormatService emailFormatService,
    ISystemNotificationService systemNotificationService) : INotificationHandler<BorrowingRequestAcceptedEvent>
{
    private async Task NotifyEmail(string bookTitle, string email, Guid borrowingRequestId, CancellationToken ct)
    {
        string subject = $"Your borrowing request has been accepted for the book {bookTitle}";

        var emailFormatResult = emailFormatService.BorrowingRequestAcceptedEmailFormat(bookTitle);
        if (emailFormatResult.IsFailure)
        {
            logger.LogWarning("Email format generation failed for email {Email}: {Errors}",
                email,
                emailFormatResult.Errors);
            return;
        }

        var emailResult = await emailService.SendEmailAsync(
            email,
            subject,
            emailFormatResult.Value);

        logger.LogInformation("Email notification sent to {Email} for borrowing request with id {BorrowingRequestId}", email, borrowingRequestId);
    }

    private async Task NotifySystem(Guid studentId, string bookTitle, CancellationToken ct)
    {
        string title = $"Your borrowing request has been accepted for the book {bookTitle}";
        string message = $"Your borrowing request has been accepted for the book {bookTitle}";

        await systemNotificationService.SendNotificationAsync(studentId, title, message, NotificationType.Good, ct);
    }

    public async Task Handle(BorrowingRequestAcceptedEvent notification, CancellationToken ct)
    {
        // notify the borrower that their request has been accepted
        var borrowerDataResult = await
            context.BorrowingRequests
            .Where(br => br.Id == notification.BorrowingRequestId)
            .Select(br => new
            {
                email = br.BorrowingStudent!.UniversityMail.Value,
                bookTitle = br.LendingRecord!.BookCopy!.Book!.Title.Value,
                borrowerId = br.BorrowingStudentId
            })
            .FirstOrDefaultAsync(ct);

        if (borrowerDataResult is null)
        {
            logger.LogError("BorrowingRequestAcceptedEventHandler: No borrowing request found for student with id {StudentId}", notification.BorrowingStudentId);
            return;
        }

        await NotifyEmail(borrowerDataResult.bookTitle, borrowerDataResult.email, notification.BorrowingRequestId, ct);
        await NotifySystem(borrowerDataResult.borrowerId, borrowerDataResult.bookTitle, ct);
    }
}
