namespace BookOrbit.Api.Controllers.LendingList;

[Route("api/v{version:apiVersion}/lendinglist")]
[ApiVersion("1.0")]
[Authorize]

public class LendingListController(
    ISender sender) : ApiController
{
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(LendingListRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a lending list record by identifier.")]
    [EndpointDescription("Returns the detailed lending list record for the specified identifier when the record exists.")]
    [EndpointName("GetLendingListRecordById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<LendingListRecordDto>> GetLendingListRecordById([FromRoute]Guid id,CancellationToken ct)
    {
        var query = new GetLendingListRecordByIdQuery(id);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet()]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<LendingListRecordListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of lending list records.")]
    [EndpointDescription("Returns a paginated collection of lending list records and supports searching, filtering, and sorting by [createdat,updatedat,cost,borrowingduration,expirationdate,state] so users can review lending activity efficiently.")]
    [EndpointName("GetLendingListRecords")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
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