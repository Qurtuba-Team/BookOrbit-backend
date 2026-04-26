using BookOrbit.Domain.BorrowingRequests.DomainEvents;

namespace BookOrbit.Application.Features.BorrowingRequests.EventHandlers;
public class BorrowingRequestAcceptedEventHandler(
    IEmailService emailService,
    IAppDbContext context,
    ILogger<BorrowingRequestCreatedEventHandler> logger,
    IEmailFormatService emailFormatService) : INotificationHandler<BorrowingRequestAcceptedEvent>
{
    public async Task Handle(BorrowingRequestAcceptedEvent notification, CancellationToken ct)
    {
        //Notify The Borrower that their request has been accepted

        var borrowerDataResult = await
            context.BorrowingRequests
            .Where(br => br.BorrowingStudentId == notification.BorrowingStudentId)
            .Select(
                br => new
                {
                    email = br.BorrowingStudent!.UniversityMail.Value,
                    bookTitle = br.LendingRecord!.BookCopy!.Book!.Title.Value
                })
            .FirstOrDefaultAsync(ct);

        if(borrowerDataResult is null)
        {
            logger.LogError("BorrowingRequestAcceptedEventHandler: No borrowing request found for student with id {studentId}", notification.BorrowingStudentId);
            return;
        }

        string subject = $"Your borrowing request has been accepted for the book {borrowerDataResult.bookTitle}";

        var emailFormatResult = emailFormatService.BorrowingRequestAcceptedEmailFormat(borrowerDataResult.bookTitle);
        if (emailFormatResult.IsFailure)
        {
            logger.LogWarning("Email format generation failed for email {Email}: {Errors}",
                borrowerDataResult.email,
                emailFormatResult.Errors);
            return;
        }


        var emailResult = await emailService.SendEmailAsync(
            borrowerDataResult.email,
            subject,
            emailFormatResult.Value
        );

        //Dont Use Emali Result , even if the email fails to send, we dont want to fail the borrowing request creation process, we just log the error and move on
        //Dont Log Here , Email Service should handle the logging of email sending success or failure, we just log the fact that we attempted to send an email notification for the borrowing request creation event

        logger.LogInformation("Email notification sent to {Email} for borrowing request with id {BorrowingRequestId}", borrowerDataResult.email, notification.BorrowingRequestId);
    }
}