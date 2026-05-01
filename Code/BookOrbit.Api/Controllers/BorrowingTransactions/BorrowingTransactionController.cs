using BookOrbit.Application.Features.OTPs.SendBookReturningConfirmationOtp;
using BookOrbit.Application.Features.OTPs.VerifyBookReturningConfirmationOtp;

namespace BookOrbit.Api.Controllers.BorrowingTransactions;

[Route("api/v{version:apiVersion}/borrowingtransactions")]
[ApiVersion("1.0")]
[Authorize]
public class BorrowingTransactionController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    [HttpGet("{borrowingTransactionId:guid}", Name = "GetBorrowingTransactionById")]
    [Authorize(Policy = PoliciesNames.BorrowingTransactionRelatedStudentPolicy)]
    [ProducesResponseType(typeof(BorrowingTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a borrowing transaction by identifier.")]
    [EndpointDescription("Returns the borrowing transaction details for the specified identifier when the transaction exists.")]
    [EndpointName("GetBorrowingTransactionById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BorrowingTransactionDto>> GetBorrowingTransactionById([FromRoute] Guid borrowingTransactionId, CancellationToken ct)
    {
        var query = new GetBorrowingTransactionByIdQuery(borrowingTransactionId);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BorrowingTransactionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of borrowing transactions.")]
    [EndpointDescription("Returns a paginated collection of borrowing transactions and supports searching, filtering, and sorting by [createdat,updatedat,expectedreturndate,actualreturndate,state,borrowername,lendername,booktitle] so administrators can review borrowing activity efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBorrowingTransactions")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<BorrowingTransactionListItemDto>>> GetBorrowingTransactions([FromQuery] BorrowingTransactionPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetBorrowingTransactionsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.BorrowerStudentId,
            request.LenderStudentId,
            request.BookCopyId,
            request.BorrowingRequestId,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet("me/in")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BorrowingTransactionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of borrowing transactions that is incomming.")]
    [EndpointDescription("Returns a paginated collection of borrowing transactions and supports searching, filtering, and sorting by [createdat,updatedat,expectedreturndate,actualreturndate,state,borrowername,lendername,booktitle] so administrators can review borrowing activity efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetIncommingBorrowingTransactions")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<BorrowingTransactionListItemDto>>> GetIncommingBorrowingTransactions([FromQuery] BorrowingTransactionPagedFilterRequest request, CancellationToken ct)
    {
        var studentResult = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentResult.IsFailure)
        {
            return Problem(studentResult.Errors, HttpContext);
        }


        var query = new GetBorrowingTransactionsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.BorrowerStudentId,
            studentResult.Value.Id,
            request.BookCopyId,
            request.BorrowingRequestId,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }


    [HttpGet("me/out")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BorrowingTransactionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of borrowing transactions that is outgoing.")]
    [EndpointDescription("Returns a paginated collection of borrowing transactions and supports searching, filtering, and sorting by [createdat,updatedat,expectedreturndate,actualreturndate,state,borrowername,lendername,booktitle] so administrators can review borrowing activity efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetOutgoingBorrowingTransactions")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<BorrowingTransactionListItemDto>>> GetOutgoingBorrowingTransactions([FromQuery] BorrowingTransactionPagedFilterRequest request, CancellationToken ct)
    {
        var studentResult = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentResult.IsFailure)
        {
            return Problem(studentResult.Errors, HttpContext);
        }

        var query = new GetBorrowingTransactionsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            studentResult.Value.Id,
            request.LenderStudentId,
            request.BookCopyId,
            request.BorrowingRequestId,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }



    [HttpPatch("{borrowingTransactionId:guid}/return")]
    [Authorize(Policy = PoliciesNames.BorrowingTransactionBorrowingStudentPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Mark a borrowing transaction as returned.")]
    [EndpointDescription("Marks the specified borrowing transaction as returned when the borrower is authorized and the transaction is in a valid state.")]
    [EndpointName("MarkBorrowingTransactionAsReturned")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> MarkBorrowingTransactionAsReturned([FromRoute] Guid borrowingTransactionId, [FromBody] OtpRequest request, CancellationToken ct)
    {
        var otpVerificationResult = await sender.Send(new VerifyBookReturningConfirmationOtpCommand(borrowingTransactionId, request.OtpCode), ct);

        if (otpVerificationResult.IsFailure)
        {
            return Problem(otpVerificationResult.Errors, HttpContext);
        }

        var result = await sender.Send(new MarkAsReturnedBorrowingTransactionCommand(borrowingTransactionId), ct);

        return result.Match(
            _ => NoContent(),
            e => Problem(e, HttpContext));
    }

    [HttpPatch("{borrowingTransactionId:guid}/lost")]
    [Authorize(Policy = PoliciesNames.BorrowingTransactionBorrowingStudentPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Mark a borrowing transaction as lost.")]
    [EndpointDescription("Marks the specified borrowing transaction as lost when the borrower is authorized and the transaction is in a valid state.")]
    [EndpointName("MarkBorrowingTransactionAsLost")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> MarkBorrowingTransactionAsLost([FromRoute] Guid borrowingTransactionId, CancellationToken ct)
    {
        var result = await sender.Send(new MarkAsLostBorrowingTransactionCommand(borrowingTransactionId), ct);

        return result.Match(
            _ => NoContent(),
            e => Problem(e, HttpContext));
    }

    [HttpPost("{borrowingTransactionId:guid}/otp")]
    [Authorize(Policy = PoliciesNames.BorrowingTransactionBorrowingStudentPolicy)]
    [ProducesResponseType(typeof(OtpDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Send an otp code to confirm returning.")]
    [EndpointDescription("Send an otp code to confirm returning.")]
    [EndpointName("SendBookReturningOtp")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<OtpDto>> SendBookReturningOtp([FromRoute] Guid borrowingTransactionId, CancellationToken ct)
    {
        var command = new SendBookReturningConfirmationOtpCommand(borrowingTransactionId);

        var result = await sender.Send(command, ct);

        return result.Match(
            _ => Ok(),
            e => Problem(e, HttpContext));
    }
}