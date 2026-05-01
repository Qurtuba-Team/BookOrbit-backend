namespace BookOrbit.Api.Controllers.BorrowingTransactionEvents;

[Route("api/v{version:apiVersion}/borrowingtransactionevents")]
[ApiVersion("1.0")]
[Authorize]
public class BorrowingTransactionEventController(
    ISender sender) : ApiController
{
    [HttpGet("{borrowingTransactionEventId:guid}", Name = "GetBorrowingTransactionEventById")]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(typeof(BorrowingTransactionEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a borrowing transaction event by identifier.")]
    [EndpointDescription("Returns the borrowing transaction event details for the specified identifier when the event exists.")]
    [EndpointName("GetBorrowingTransactionEventById")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BorrowingTransactionEventDto>> GetBorrowingTransactionEventById([FromRoute] Guid borrowingTransactionEventId, CancellationToken ct)
    {
        var query = new GetBorrowingTransactionEventByIdQuery(borrowingTransactionEventId);

        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }

    [HttpGet]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BorrowingTransactionEventListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of borrowing transaction events.")]
    [EndpointDescription("Returns a paginated collection of borrowing transaction events and supports filtering and sorting by [createdat,updatedat,state] so administrators can review the event history efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBorrowingTransactionEvents")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<BorrowingTransactionEventListItemDto>>> GetBorrowingTransactionEvents([FromQuery] BorrowingTransactionEventPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetBorrowingTransactionEventsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.BorrowingTransactionId,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }
}
