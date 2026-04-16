namespace BookOrbit.Api.Controllers.BookCopies;

[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[Authorize]
public class BookCopyController(
    ISender sender) : ApiController
{

    [HttpPost("students/{id:guid}/books/copies")]
    [Authorize(Policy = PoliciesNames.StudentOwnershipPolicy)]
    [ProducesResponseType(typeof(BookCopyDtoWithBookDetails), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Create a new book copy record.")]
    [EndpointDescription("Creates a new book copy for the specified student and links it to an existing book so the physical copy can be tracked by the system.")]
    [EndpointName("CreateBookCopy")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BookCopyDtoWithBookDetails>> CreateBookCopy([FromRoute] Guid id, [FromBody] CreateBookCopyRequest request, CancellationToken ct)
    {
        var command = new CreateBookCopyCommand(
            id,
            request.BookId,
            request.Condition);

        var result = await sender.Send(command, ct);

        return result.Match(
       bookDto => CreatedAtRoute(
       routeName: "GetBookCopyById",
       routeValues: new { version = "1.0", id = bookDto.Id },
       value: bookDto),

       e => Problem(e, HttpContext));
    }


    [HttpPatch("books/copies/{id:guid}")]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Update an existing book copy record.")]
    [EndpointDescription("Updates the editable information for the specified book copy, such as its condition, so copy tracking remains accurate.")]
    [EndpointName("UpdateBookCopy")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> UpdateBookCopy([FromRoute] Guid id, [FromBody] UpdateBookCopyRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateBookCopyCommand(
                id,
            request.Condition),
            ct);

        return result.Match(
           response => NoContent(),
           e => Problem(e, HttpContext));
    }



    [HttpGet("books/copies/{id:guid}", Name = "GetBookCopyById")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(BookCopyDtoWithBookDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a book copy by identifier.")]
    [EndpointDescription("Returns the detailed information for the specified book copy, including the related book details, when the record exists.")]
    [EndpointName("GetBookCopyById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BookCopyDtoWithBookDetails>> GetBookCopyById([FromRoute] Guid id, CancellationToken ct)
    {
        var query = new GetBookCopyByIdQuery(id);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }


    [HttpGet("books/{id:guid}/copies")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BookCopyListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve book copies for a specific book.")]
    [EndpointDescription("Returns a paginated collection of book copies that belong to the specified book and supports filtering, searching, and sorting By [createdat,ownername,booktitle,updatedat]  for easier inventory review.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBookCopiesByBookId")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BookCopyListItemDto>> GetBookCopiesByBookId([FromRoute] Guid id, [FromQuery] BookCopyPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetBookCopiesQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            BookId: id,
            OwnerId: null,
            request.Conditions,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

    [HttpGet("students/{id:guid}/books/copies")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BookCopyListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve book copies owned by a specific student.")]
    [EndpointDescription("Returns a paginated collection of book copies owned by the specified student and supports filtering, searching, and sorting By [createdat,ownername,booktitle,updatedat]  for easier account review.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBookCopiesByStudentId")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BookCopyListItemDto>> GetBookCopiesByStudentId([FromRoute] Guid id, [FromQuery] BookCopyPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetBookCopiesQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            BookId: null,
            OwnerId: id,
            request.Conditions,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }


    [HttpGet("books/copies")]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BookCopyListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of book copies.")]
    [EndpointDescription("Returns a paginated collection of all book copies and supports filtering, searching, and sorting so administrators can manage inventory efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBookCopies")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BookCopyListItemDto>> GetBookCopies([FromQuery] BookCopyPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetBookCopiesQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            BookId: null,
            OwnerId: null,
            request.Conditions,
            request.States);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

}
