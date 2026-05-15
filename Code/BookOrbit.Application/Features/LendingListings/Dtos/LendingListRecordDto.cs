namespace BookOrbit.Application.Features.LendingListings.Dtos;
public record LendingListRecordDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BookCopyId { get; set; } = Guid.Empty;
    public LendingListRecordState State { get; set; }
    public int BorrowingDurationInDays { get; set; }
    public int Cost { get; set; }
    public byte[] RowVersion { get; set; } = [];

    [JsonConstructor]
    private LendingListRecordDto() { }

    private LendingListRecordDto(
        Guid id,
        Guid bookCopyId,
        LendingListRecordState state,
        int borrowingDurationInDays,
        int cost,
        byte[] rowVersion)
    {
        Id = id;
        BookCopyId = bookCopyId;
        State = state;
        BorrowingDurationInDays = borrowingDurationInDays;
        Cost = cost;
        RowVersion = rowVersion;
    }

    static public LendingListRecordDto FromEntity(LendingListRecord lendingListRecord)
    {
        return new LendingListRecordDto(
            lendingListRecord.Id,
            lendingListRecord.BookCopyId,
            lendingListRecord.State,
            lendingListRecord.BorrowingDurationInDays,
            lendingListRecord.Cost.Value,
            lendingListRecord.RowVersion
        );
    }
}