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
                s.LendingRecord!.Id,
                s.BorrowingStudentId,
                s.LendingRecord!.State,
                s.LendingRecord!.BookCopy!.OwnerId,
                s.LendingRecord!.BookCopy!.Owner!.PhoneNumber,
                s.LendingRecord!.BookCopy!.Owner!.TelegramUserId
            })
            .FirstOrDefaultAsync(s => s.Id == query.LendingListId, ct);

        if (studentData is null)
        {
            logger.LogWarning("Lending List Record {LendingListId} not found .", query.LendingListId);

            return LendingListApplicationErrors.NotFoundById;
        }

        if(studentData.State is not LendingListRecordState.Reserved)
        {
            logger.LogWarning("Lending List Record {LendingListId} is not in Reserved state.", query.LendingListId);
            return LendingListApplicationErrors.StateIsNotReserved;
        }

        if (studentData.BorrowingStudentId != query.StudentId)
        {
            logger.LogWarning("Student {StudentId} is not the owner of Lending List Record {LendingListId}.", query.StudentId, query.LendingListId);
            return LendingListApplicationErrors.NotFoundById;
        }

        return new StudentContactInformationDto(
            studentData.OwnerId,
            studentData.PhoneNumber?.Value,
            studentData.TelegramUserId?.Value);
    }
}
