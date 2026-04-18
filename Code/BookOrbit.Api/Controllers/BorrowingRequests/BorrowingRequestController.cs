namespace BookOrbit.Api.Controllers.BorrowingRequests;

[Route("api/v{version:apiVersion}/borrowingrequests")]
[ApiVersion("1.0")]
[Authorize]
public class BorrowingRequestController(
    ISender sender) : ApiController
{
    [HttpGet("{borrowingRequestId:guid}", Name = "GetBorrowingRequestById")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(BorrowingRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a borrowing request by identifier.")]
    [EndpointDescription("Returns the borrowing request details for the specified identifier when the request exists.")]
    [EndpointName("GetBorrowingRequestById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BorrowingRequestDto>> GetBorrowingRequestById([FromRoute] Guid borrowingRequestId, CancellationToken ct)
    {
        var query = new GetBorrowingRequestByIdQuery(borrowingRequestId);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BorrowingRequestListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of borrowing requests.")]
    [EndpointDescription("Returns a paginated collection of borrowing requests and supports searching, filtering, and sorting by [createdat,updatedat,expirationdate,state,borrowername,booktitle] so clients can review borrowing activity efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBorrowingRequests")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<BorrowingRequestListItemDto>>> GetBorrowingRequests([FromQuery] BorrowingRequestPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetBorrowingRequestsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.BorrowingStudentId,
            request.LendingListRecordId,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }
}
