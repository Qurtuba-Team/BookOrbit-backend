using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.CancelBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.RejectBorrowingRequest;
using BookOrbit.Application.Features.BorrowingTransactions.Commands.CreateBorrowingTransaction;
using BookOrbit.Application.Features.BorrowingTransactions.Dtos;

namespace BookOrbit.Api.Controllers.BorrowingRequests;

[Route("api/v{version:apiVersion}/borrowingrequests")]
[ApiVersion("1.0")]
[Authorize]
public class BorrowingRequestController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    [HttpGet("{borrowingRequestId:guid}", Name = "GetBorrowingRequestById")]
    [Authorize(Policy = PoliciesNames.BorrowingRequestRelatedStudentPolicy)]
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
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
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
            request.LendingStudentId,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet("me/in")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BorrowingRequestListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of incoming borrowing requests.")]
    [EndpointDescription("Returns a paginated collection of incoming borrowing requests for the current student and supports searching, filtering, and sorting by [createdat,updatedat,expirationdate,state,borrowername,booktitle].")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetIncomingBorrowingRequests")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<BorrowingRequestListItemDto>>> GetIncomingBorrowingRequests([FromQuery] BorrowingRequestPagedFilterRequest request, CancellationToken ct)
    {
        var studentResult = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentResult.IsFailure)
        {
            return Problem(studentResult.Errors, HttpContext);
        }


        var query = new GetBorrowingRequestsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.BorrowingStudentId,
            request.LendingListRecordId,
            studentResult.Value.Id,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet("me/out")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BorrowingRequestListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of outgoing borrowing requests.")]
    [EndpointDescription("Returns a paginated collection of outgoing borrowing requests for the current student and supports searching, filtering, and sorting by [createdat,updatedat,expirationdate,state,borrowername,booktitle].")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetOutgoingBorrowingRequests")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<BorrowingRequestListItemDto>>> GetOutgoingBorrowingRequests([FromQuery] BorrowingRequestPagedFilterRequest request, CancellationToken ct)
    {
        var studentResult = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentResult.IsFailure)
        {
            return Problem(studentResult.Errors, HttpContext);
        }

        var query = new GetBorrowingRequestsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            studentResult.Value.Id,
            request.LendingListRecordId,
            request.LendingStudentId,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }


    [HttpPatch("{borrowingRequestId:guid}/accept")]
    [Authorize(Policy = PoliciesNames.BorrowingRequestLendingStudentPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Accept a borrowing request.")]
    [EndpointDescription("Marks the specified borrowing request as accepted when it is in a valid pending state.")]
    [EndpointName("AcceptBorrowingRequest")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> AcceptBorrowingRequest([FromRoute] Guid borrowingRequestId, CancellationToken ct)
    {
        var result = await sender.Send(new AcceptBorrowingRequestCommand(borrowingRequestId), ct);

        return result.Match(
            _ => NoContent(),
            e => Problem(e, HttpContext));
    }


    [HttpPatch("{borrowingRequestId:guid}/reject")]
    [Authorize(Policy = PoliciesNames.BorrowingRequestLendingStudentPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Reject a borrowing request.")]
    [EndpointDescription("Rejects the specified borrowing request when it is in a valid pending state.")]
    [EndpointName("RejectBorrowingRequest")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> RejectBorrowingRequest([FromRoute] Guid borrowingRequestId, CancellationToken ct)
    {
       var result = await sender.Send(new RejectBorrowingRequestCommand(borrowingRequestId), ct);

        return result.Match(
            _ => NoContent(),
            e => Problem(e, HttpContext));
    }


    [HttpPatch("{borrowingRequestId:guid}/cancel")]
    [Authorize(Policy = PoliciesNames.BorrowingRequestBorrowingStudentPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Cancel a borrowing request.")]
    [EndpointDescription("Cancels the specified borrowing request when it is in a valid state for cancellation.")]
    [EndpointName("CancelBorrowingRequest")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> CancelBorrowingRequest([FromRoute] Guid borrowingRequestId, CancellationToken ct)
    {
        var result = await sender.Send(new CancelBorrowingRequestCommand(borrowingRequestId), ct);

        return result.Match(
            _ => NoContent(),
            e => Problem(e, HttpContext));
    }

    [HttpPost("{borrowingRequestId:guid}/deliver")]
    [Authorize(Policy = PoliciesNames.BorrowingRequestLendingStudentPolicy)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Mark a borrowing request as delivered.")]
    [EndpointDescription("Creates a borrowing transaction for the specified borrowing request when it is in a valid state for delivery.")]
    [EndpointName("CreateBorrowingTransaction")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BorrowingTransactionDto>> CreateBorrowingTransaction([FromRoute] Guid borrowingRequestId, CancellationToken ct)
    {
        var result = await sender.Send(new CreateBorrowingTransactionCommand(borrowingRequestId), ct);
        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }
}
