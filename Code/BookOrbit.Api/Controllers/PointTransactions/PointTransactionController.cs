
namespace BookOrbit.Api.Controllers.PointTransactions;

[Route("api/v{version:apiVersion}/pointtransactions")]
[ApiVersion("1.0")]
[Authorize]
public class PointTransactionController(
    ISender sender) : ApiController
{
    [HttpGet("{pointTransactionId:guid}", Name = "GetPointTransactionById")]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(typeof(PointTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a point transaction by identifier.")]
    [EndpointDescription("Returns the point transaction details for the specified identifier when the transaction exists.")]
    [EndpointName("GetPointTransactionById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PointTransactionDto>> GetPointTransactionById([FromRoute] Guid pointTransactionId, CancellationToken ct)
    {
        var query = new GetPointTransactionByIdQuery(pointTransactionId);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(typeof(PaginatedList<PointTransactionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of point transactions.")]
    [EndpointDescription("Returns a paginated collection of point transactions and supports searching, filtering, and sorting by [createdat,updatedat,points,reason,studentname] so administrators can review point activity efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetPointTransactions")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<PointTransactionListItemDto>>> GetPointTransactions([FromQuery] PointTransactionPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetPointTransactionsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.StudentId,
            request.BorrowingReviewId,
            request.Reasons);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }
}
