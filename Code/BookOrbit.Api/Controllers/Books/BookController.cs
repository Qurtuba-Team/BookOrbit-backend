
using BookOrbit.Application.Common.Interfaces.ImageServices;

namespace BookOrbit.Api.Controllers.Books;

using BookOrbit.Application.Features.Books.Commands.StateMachien.MakeBookAvilable;

[Route("api/v{version:apiVersion}/books")]
[ApiVersion("1.0")]
[Authorize]

public class BookController(
    ISender sender,
    IBookImageService bookImageService) : ApiController
{
    [HttpPost]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Create a new book record.")]
    [EndpointDescription("Creates a new book in the catalog using the submitted metadata and cover image so it can be referenced by book copy and listing endpoints.")]
    [EndpointName("CreateBook")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BookDto>> CreateBook([FromForm] CreateBookRequest request, CancellationToken ct)
    {
        using var stream = request.CoverImage.OpenReadStream();

        //Upload Image, Get New Image Name 
        var ImageUploadResult = await bookImageService.UploadImage(
            stream,
             request.CoverImage.FileName);

        if (ImageUploadResult.IsFailure)
            return Problem(ImageUploadResult.Errors, HttpContext);

        BookCategory? category = FlagEnumHelper.Map(request.Categories);

        //If null , use none
        category ??= BookCategory.None;


        var command = new CreateBookCommand(
            request.Title,
            request.ISBN,
            request.Publisher,
            category.Value,
            request.Author,
            ImageUploadResult.Value);

        var result = await sender.Send(command, ct);
        if (result.IsFailure)
            bookImageService.DeleteImage(ImageUploadResult.Value);

        return result.Match(
           bookDto => CreatedAtRoute(
               routeName: "GetBookById",
               routeValues: new { version = "1.0", bookId = bookDto.Id },
               value: bookDto),

           e => Problem(e, HttpContext));
    }


    [HttpPatch("{bookId:guid}")]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Update an existing book record.")]
    [EndpointDescription("Updates the editable information for the specified book so catalog data remains accurate and current across the application.")]
    [EndpointName("UpdateBook")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> UpdateBook([FromRoute] Guid bookId, [FromBody] UpdateBookRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateBookCommand(
                bookId,
            request.Title),
            ct);

        return result.Match(
           response => NoContent(),
           e => Problem(e, HttpContext));
    }


    [HttpPatch("{bookId:guid}/available")]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Mark a book as available.")]
    [EndpointDescription("Marks the specified book as available when it is in a valid state for availability.")]
    [EndpointName("MakeBookAvailable")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> MakeBookAvailable([FromRoute] Guid bookId, CancellationToken ct)
    {
        var result = await sender.Send(new MakeBookAvilableCommand(bookId), ct);

        return result.Match(
           _ => NoContent(),
           e => Problem(e, HttpContext));
    }


    [HttpDelete("{bookId:guid}")]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Delete a book record.")]
    [EndpointDescription("Deletes the specified book from the catalog when it is not referenced by any book copies.")]
    [EndpointName("DeleteBookById")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> DeleteBookById([FromRoute] Guid bookId, CancellationToken ct)
    {
        var result = await sender.Send(
            new DeleteBookCommand(
                bookId),
            ct);

        return result.Match(
           response => NoContent(),
           e => Problem(e, HttpContext));
    }



    [HttpGet("{bookId:guid}", Name = "GetBookById")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a book by identifier.")]
    [EndpointDescription("Returns the detailed catalog information for the specified book when a matching record exists.")]
    [EndpointName("GetBookById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BookDto>> GetBookById([FromRoute] Guid bookId, CancellationToken ct)
    {
        var query = new GetBookByIdQuery(bookId);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }



    [HttpGet]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<BookListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of books.")]
    [EndpointDescription("Returns a paginated collection of books and supports searching, category filtering, status filtering, and sorting by [createdat,updatedat,title,publisher,author] so clients can browse the catalog efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBooks")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<BookDto>> GetBooks([FromQuery] BookPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetBooksQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.Categories,
            request.Statuses);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }
}