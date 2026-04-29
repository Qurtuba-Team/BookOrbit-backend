using BookOrbit.Api.Contracts.Requests.BorrowingReviews;
using BookOrbit.Application.Features.BorrowingReviews.Commands.CreateBorrowingReview;
using BookOrbit.Application.Features.BorrowingReviews.Dtos;
using BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviewById;
using BookOrbit.Application.Features.BorrowingReviews.Queries.GetBorrowingReviews;

namespace BookOrbit.Api.Controllers.BorrowingReviews;

[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[Authorize]
public class BorrowingReviewController(
    ISender sender) : ApiController
{
    [HttpPost("borrowingtransactions/{borrowingTransactionId:guid}/review")]
    [Authorize(Policy = PoliciesNames.BorrowingTransactionLendingStudentPolicy)]
    [ProducesResponseType(typeof(BorrowingReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Create a borrowing review.")]
    [EndpointDescription("Creates a new borrowing review for the specified borrowing transaction.")]
    [EndpointName("CreateBorrowingReview")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BorrowingReviewDto>> CreateBorrowingReview([FromRoute] Guid borrowingTransactionId, [FromBody] CreateBorrowingReviewRequest request, CancellationToken ct)
    {
        var command = new CreateBorrowingReviewCommand(
            request.ReviewerStudentId,
            request.ReviewedStudentId,
            borrowingTransactionId,
            request.Description,
            request.Rating);

        var result = await sender.Send(command, ct);

        return result.Match(
            review => CreatedAtRoute(
                routeName: "GetBorrowingReviewById",
                routeValues: new { version = "1.0", borrowingReviewId = review.Id },
                value: review),
            e => Problem(e, HttpContext));
    }

    [HttpGet("borrowingreviews/{borrowingReviewId:guid}", Name = "GetBorrowingReviewById")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(BorrowingReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a borrowing review by identifier.")]
    [EndpointDescription("Returns the borrowing review details for the specified identifier when the review exists.")]
    [EndpointName("GetBorrowingReviewById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BorrowingReviewDto>> GetBorrowingReviewById([FromRoute] Guid borrowingReviewId, CancellationToken ct)
    {
        var query = new GetBorrowingReviewByIdQuery(borrowingReviewId);

        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }

    [HttpGet("borrowingreviews")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BorrowingReviewListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of borrowing reviews.")]
    [EndpointDescription("Returns a paginated collection of borrowing reviews and supports searching, filtering, and sorting by [createdat,updatedat,rating] so administrators can review borrowing feedback efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBorrowingReviews")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<BorrowingReviewListItemDto>>> GetBorrowingReviews([FromQuery] BorrowingReviewPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetBorrowingReviewsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.ReviewerStudentId,
            request.ReviewedStudentId,
            request.BorrowingTransactionId);

        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }
}