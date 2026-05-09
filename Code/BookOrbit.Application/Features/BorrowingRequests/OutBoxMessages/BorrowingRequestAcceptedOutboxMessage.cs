namespace BookOrbit.Application.Features.BorrowingRequests.OutBoxMessages;
public class BorrowingRequestAcceptedOutboxMessage(
    string subject,
    string emailAddress,
    string emailFormat) : OutboxMessageRecord(subject, emailAddress, emailFormat)
{
}