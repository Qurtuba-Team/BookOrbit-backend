using BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;

namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Dtos;

public record BorrowingTransactionEventDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BorrowingTransactionId { get; set; } = Guid.Empty;
    public BorrowingTransactionState State { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastModifiedUtc { get; set; }

    [JsonConstructor]
    private BorrowingTransactionEventDto() { }

    private BorrowingTransactionEventDto(
        Guid id,
        Guid borrowingTransactionId,
        BorrowingTransactionState state,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastModifiedUtc)
    {
        Id = id;
        BorrowingTransactionId = borrowingTransactionId;
        State = state;
        CreatedAtUtc = createdAtUtc;
        LastModifiedUtc = lastModifiedUtc;
    }

    public static BorrowingTransactionEventDto FromEntity(BorrowingTransactionEvent transactionEvent)
        => new(
            transactionEvent.Id,
            transactionEvent.BorrowingTransactionId,
            transactionEvent.State,
            transactionEvent.CreatedAtUtc,
            transactionEvent.LastModifiedUtc);
}
