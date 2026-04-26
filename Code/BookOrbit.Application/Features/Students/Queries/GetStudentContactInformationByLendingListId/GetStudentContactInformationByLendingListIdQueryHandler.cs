namespace BookOrbit.Application.Features.Students.Queries.GetStudentContactInformationByLendingListId;

public class GetStudentContactInformationByLendingListIdQueryHandler
    (ILogger<GetStudentContactInformationByLendingListIdQueryHandler> logger,
    IAppDbContext context)
    : IRequestHandler<GetStudentContactInformationByLendingListIdQuery, Result<StudentContactInformationDto>>
{
    public async Task<Result<StudentContactInformationDto>> Handle(GetStudentContactInformationByLendingListIdQuery query, CancellationToken ct)
    {
        var studentData = await context.BorrowingRequests
            .AsNoTracking()
            .Select(s => new
            {
                LendingRecordId = s.LendingRecord!.Id,
                s.BorrowingStudentId,
                s.LendingRecord!.State,
                s.LendingRecord!.BookCopy!.OwnerId,
                s.LendingRecord!.BookCopy!.Owner!.PhoneNumber,
                s.LendingRecord!.BookCopy!.Owner!.TelegramUserId
            })
            .FirstOrDefaultAsync(s => s.LendingRecordId == query.LendingListId, ct);

        if (studentData is null)
        {
            logger.LogWarning("Lending List Record {LendingListId} not found .", query.LendingListId);

            return LendingListApplicationErrors.NotFoundById;
        }

        if (studentData.State is not LendingListRecordState.Reserved)
        {
            logger.LogWarning("Lending List Record {LendingListId} is not in Reserved state.", query.LendingListId);
            return LendingListApplicationErrors.StateIsNotReserved;
        }
        return new StudentContactInformationDto(
            studentData.OwnerId,
            studentData.PhoneNumber?.Value,
            studentData.TelegramUserId?.Value);
    }
}
