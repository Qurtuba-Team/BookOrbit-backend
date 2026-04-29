
namespace BookOrbit.Application.Features.BorrowingTransactionEvents.Dtos;

public record BorrowingTransactionEventListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BorrowingTransactionId { get; set; } = Guid.Empty;
    public BorrowingTransactionState State { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonConstructor]
    private BorrowingTransactionEventListItemDto() { }

    private BorrowingTransactionEventListItemDto(
        Guid id,
        Guid borrowingTransactionId,
        BorrowingTransactionState state,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        BorrowingTransactionId = borrowingTransactionId;
        State = state;
        CreatedAtUtc = createdAtUtc;
    }

    public static BorrowingTransactionEventListItemDto FromEntity(BorrowingTransactionEvent transactionEvent)
        => new(
            transactionEvent.Id,
            transactionEvent.BorrowingTransactionId,
            transactionEvent.State,
            transactionEvent.CreatedAtUtc);
}
