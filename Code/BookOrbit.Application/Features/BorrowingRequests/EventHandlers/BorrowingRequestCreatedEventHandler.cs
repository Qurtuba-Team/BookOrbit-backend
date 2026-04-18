using BookOrbit.Domain.BorrowingRequests.DomainEvents;

namespace BookOrbit.Application.Features.BorrowingRequests.EventHandlers;
public class BorrowingRequestCreatedEventHandler(
    IEmailService emailService,
    IAppDbContext context,
    ILogger<BorrowingRequestCreatedEventHandler>logger) : INotificationHandler<BorrowingRequestCreatedEvent>
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
        string body = $"A new borrowing request has been created for your copy of the book: {ownerDataResult.Booktitle}. Please log in to your account to review the request and take appropriate action.";
        await emailService.SendEmailAsync(
            ownerDataResult.email,
            subject,
            body
        );

        logger.LogInformation("Email notification sent to {Email} for borrowing request with id {BorrowingRequestId} and lending record id {LendingRecordId}", ownerDataResult.email, notification.BorrowingRequestId, notification.LendingRecordId);
    }
}