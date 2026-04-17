namespace BookOrbit.Api.Controllers.LendingList;

[Route("api/v{version:apiVersion}/lendinglist")]
[ApiVersion("1.0")]
[Authorize]

public class LendingListController(
    ISender sender) : ApiController
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LendingListRecordDto>> GetLendingListRecordById([FromRoute]Guid id,CancellationToken ct)
    {
        var query = new GetLendingListRecordByIdQuery(id);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet()]
    public async Task<ActionResult<PaginatedList<LendingListRecordListItemDto>>> GetLendingListRecords([FromQuery] LendingListPagedFilter request,CancellationToken ct)
    {
        var query = new GetLendingListRecordsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.BookCopyId,
            request.BookId,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

}