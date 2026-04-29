
namespace BookOrbit.Application.Features.BorrowingRequests.EventHandlers;
public class BorrowingRequestCreatedEventHandler(
    IEmailService emailService,
    IAppDbContext context,
    ILogger<BorrowingRequestCreatedEventHandler>logger,
    IEmailFormatService emailFormatService) : INotificationHandler<BorrowingRequestCreatedEvent>
{
    public async Task Handle(BorrowingRequestCreatedEvent notification, CancellationToken ct)
    {
        //notify the owner of the book that a borrowing request has been created
        var ownerDataResult = await
            context.LendingListRecords.
            Where(llr => llr.Id == notification.LendingRecordId)
            .Select(
            llr => new
            {
            email = llr.BookCopy!.Owner!.UniversityMail.Value,
            Booktitle = llr.BookCopy!.Book!.Title.Value
            })
            .FirstOrDefaultAsync(ct);

        if(ownerDataResult is null)
        {
            //log error that the owner data could not be found for the borrowing request
            logger.LogError("Owner data could not be found for borrowing request with id {BorrowingRequestId} and lending record id {LendingRecordId}", notification.BorrowingRequestId, notification.LendingRecordId);

            return;
        }

        string subject = $"New borrowing request for your copy of the book: {ownerDataResult.Booktitle}";

        var emailFormatResult = emailFormatService.BookCopyRequestedEmailFormat(ownerDataResult.Booktitle);

        if (emailFormatResult.IsFailure)
        {
            logger.LogWarning("Email format generation failed for email {Email}: {Errors}",
                ownerDataResult.email,
                emailFormatResult.Errors);
            return;
        }


        var emailResult = await emailService.SendEmailAsync(
            ownerDataResult.email,
            subject,
            emailFormatResult.Value
        );

        //Dont Use Emali Result , even if the email fails to send, we dont want to fail the borrowing request creation process, we just log the error and move on
        //Dont Log Here , Email Service should handle the logging of email sending success or failure, we just log the fact that we attempted to send an email notification for the borrowing request creation event

        logger.LogInformation("Email notification sent to {Email} for borrowing request with id {BorrowingRequestId} and lending record id {LendingRecordId}", ownerDataResult.email, notification.BorrowingRequestId, notification.LendingRecordId);
    }
}
