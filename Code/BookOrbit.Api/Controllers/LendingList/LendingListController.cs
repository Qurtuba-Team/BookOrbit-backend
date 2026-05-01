namespace BookOrbit.Api.Controllers.LendingList;

[Route("api/v{version:apiVersion}/lendinglist")]
[ApiVersion("1.0")]
[Authorize]

public class LendingListController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    [HttpGet("{lendingListRecordId:guid}")]
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
    public async Task<ActionResult<LendingListRecordDto>> GetLendingListRecordById([FromRoute] Guid lendingListRecordId, CancellationToken ct)
    {
        var query = new GetLendingListRecordByIdQuery(lendingListRecordId);

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

    [HttpPost("{lendingListRecordId:guid}/request")]
    [Authorize(Policy = PoliciesNames.StudentOnlyPolicy)]
    [ProducesResponseType(typeof(BorrowingRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Create a borrowing request for a lending list record.")]
    [EndpointDescription("Creates a borrowing request for the specified lending list record so the borrowing workflow can proceed when the record is available.")]
    [EndpointName("CreateBorrowingRequest")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BorrowingRequestDto>> CreateBorrowingRequest([FromRoute] Guid lendingListRecordId,CancellationToken ct)
    {
        var studentFound = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentFound.IsFailure)
        {
            return Problem(studentFound.Errors, HttpContext);
        }

        var command = new CreateBorrowingRequestCommand(
            studentFound.Value.Id,
            lendingListRecordId);

        var result = await sender.Send(command, ct);

        return result.Match(
           borrowingRequest => CreatedAtRoute(
               routeName: "GetBorrowingRequestById",
               routeValues: new { version = "1.0", borrowingRequestId = borrowingRequest.Id },
               value: borrowingRequest),
           e => Problem(e, HttpContext));
    }

    [HttpGet("{lendingListRecordId:guid}/contact-info")]
    [Authorize(Policy = PoliciesNames.StudentAcceptedForLendingListRecordPolicy)]
    [ProducesResponseType(typeof(StudentContactInformationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a student's contact information by lending list record identifier.")]
    [EndpointDescription("Returns the contact information for the specified lending list record identifier when the record exists.")]
    [EndpointName("GetStudentContactInformationByLendingListId")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<StudentContactInformationDto>> GetStudentContactInformationById([FromRoute] Guid lendingListRecordId, CancellationToken ct)
    {
        var studentFound = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentFound.IsFailure)
        {
            return Problem(studentFound.Errors, HttpContext);
        }

        var query = new GetStudentContactInformationByLendingListIdQuery(lendingListRecordId, studentFound.Value.Id);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }


    [HttpPost("{lendingListRecordId:guid}/close")]
    [Authorize(Policy = PoliciesNames.StudentOwnerOfLendingListRecordPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Close a lending list record.")]
    [EndpointDescription("Closes the specified lending list record, preventing further borrowing requests.")]
    [EndpointName("CloseLendingListRecord")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BorrowingRequestDto>> CloseLendingListRecord([FromRoute] Guid lendingListRecordId, CancellationToken ct)
    {
       var command = new CloseLendingListRecordCommand(lendingListRecordId);

        var result = await sender.Send(command, ct);
        return result.Match(
           _ => NoContent(),
           e => Problem(e, HttpContext));
    }

}
