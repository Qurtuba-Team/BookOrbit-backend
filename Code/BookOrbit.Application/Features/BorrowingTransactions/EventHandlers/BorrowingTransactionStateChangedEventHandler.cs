
using BookOrbit.Domain.BorrowingTransactions.DomainEvents;

namespace BookOrbit.Application.Features.BorrowingTransactions.EventHandlers;
public class BorrowingTransactionStateChangedEventHandler(
    IAppDbContext context,
    ILogger<BorrowingTransactionStateChangedEventHandler> logger) : INotificationHandler<BorrowingTransactionStateChangedEvent>
{
    public Task Handle(BorrowingTransactionStateChangedEvent notification, CancellationToken cancellationToken)
    {
        var logCreationResult = BorrowingTransactionEvent.Create(
            Guid.NewGuid(),
            notification.BorrowingTransactionId,
            notification.State);

        if (logCreationResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to create borrowing transaction event for transaction {BorrowingTransactionId}. Errors: {Errors}",
                notification.BorrowingTransactionId,
                logCreationResult.Errors);
            return Task.CompletedTask;
        }

        context.BorrowingTransactionEvents.Add(logCreationResult.Value);
        
        logger.LogInformation(
            "Borrowing transaction log event created for transaction {BorrowingTransactionId} with state {State}",
            notification.BorrowingTransactionId,
            notification.State);
            
        return Task.CompletedTask;
    }
}
