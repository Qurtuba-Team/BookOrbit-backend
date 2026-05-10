using BookOrbit.Application.Common.Interfaces.SystemNotificationService;

namespace BookOrbit.Application.Features.BorrowingRequests.EventHandlers;
public class BorrowingRequestCreatedEventHandler(
    IAppDbContext context,
    ILogger<BorrowingRequestCreatedEventHandler>logger,
    IEmailFormatService emailFormatService,
    ISystemNotificationService systemNotificationService,
    IOutboxMessageService outboxMessageService) : INotificationHandler<BorrowingRequestCreatedEvent>
{

    private async Task NotifyEmail(string Booktitle,string Email, CancellationToken ct)
    {
        string subject = $"New borrowing request for your copy of the book: {Booktitle}";

        var emailFormatResult = emailFormatService.BookCopyRequestedEmailFormat(Booktitle);

        if (emailFormatResult.IsFailure)
        {
            logger.LogWarning("Email format generation failed for email {Email}: {Errors}",
                Email,
                emailFormatResult.Errors);
            return;
        }

        var outboxMessageResult = await outboxMessageService.AddOutboxMessageAsync(new OutboxMessageRecord(
            subject: subject,
            emailAddress: Email,
            emailFormat: emailFormatResult.Value
        ), ct);

        //log inside service
    }
    private async Task NotifySystem(Guid StudentId,string BookTitle, CancellationToken ct)
    {
        string title = $"New borrowing request for your copy of the book {BookTitle}";
        string message = $"A student has requested your copy of the book {BookTitle}";

        await systemNotificationService.SendNotificationAsync(StudentId,title,message,NotificationType.Normal, ct);
    }
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
                Booktitle = llr.BookCopy!.Book!.Title.Value,
                OwnerId = llr.BookCopy!.Owner!.Id
            })
            .FirstOrDefaultAsync(ct);

        if (ownerDataResult is null)
        {
            logger.LogError("Owner data could not be found for borrowing request with id {BorrowingRequestId} and lending record id {LendingRecordId}", notification.BorrowingRequestId, notification.LendingRecordId);

            return;
        }

        await NotifyEmail(ownerDataResult.Booktitle, ownerDataResult.email, ct);
        await NotifySystem(ownerDataResult.OwnerId,ownerDataResult.Booktitle, ct);
    }
}