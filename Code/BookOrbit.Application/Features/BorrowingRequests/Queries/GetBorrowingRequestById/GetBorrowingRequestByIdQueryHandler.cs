namespace BookOrbit.Application.Features.BorrowingRequests.Queries.GetBorrowingRequestById;
public class GetBorrowingRequestByIdQueryHandler(
    ILogger<GetBorrowingRequestByIdQueryHandler> logger,
    IAppDbContext context) : IRequestHandler<GetBorrowingRequestByIdQuery, Result<BorrowingRequestDto>>
{
    public async Task<Result<BorrowingRequestDto>> Handle(GetBorrowingRequestByIdQuery query, CancellationToken ct)
    {
        var borrowingRequest = await context.BorrowingRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(br => 
            (br.Id == query.BorrowingRequestId)
            && (
            (br.BorrowingStudentId == query.StudentId)
            ||
            (br.LendingRecord!.BookCopy!.OwnerId == query.StudentId)), ct);

        if (borrowingRequest is null)
        {
            logger.LogWarning("Borrowing request {BorrowingRequestId} not found.", query.BorrowingRequestId);
            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        return BorrowingRequestDto.FromEntity(borrowingRequest);
    }
}
