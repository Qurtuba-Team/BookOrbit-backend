


namespace BookOrbit.Api.Controllers;

[Route("api/v{version:apiVersion}/images")]
[ApiController]
[Authorize]

[ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

public class ImagesController
    (ISender sender,
    IStudentImageService studentImageService,
    IBookImageService bookImageService): ApiController
{

    [HttpGet("students/{studentId:guid}")]
    [Authorize(Policy = PoliciesNames.RegisteredUserPolicy)]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a student's profile image.")]
    [EndpointDescription("Returns the profile image file associated with the specified student. If no image is available, the request returns the appropriate error response instead of student details.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetStudentImage")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    [ResponseCache(Duration = ApiConstants.ImagesResponseCacheDurationInSeconds, Location = ResponseCacheLocation.Client)] // Store in browser
    public async Task<ActionResult> GetStudentImage([FromRoute]Guid studentId)
    {
        var fileNameResult = await sender.Send(new GetStudentPersonalPhotoFileNameByIdQuery(
            studentId));

        if (fileNameResult.IsFailure)
            return Problem(fileNameResult.Errors, HttpContext);


        var extension = Path.GetExtension(fileNameResult.Value).ToLower();

        var image = await studentImageService.GetImage(fileNameResult.Value);

        if (image == null)
            return NotFound("Image Not Found");

        return File(image, ImageHelper.GetContentType(extension));
    }


    [HttpGet("books/{bookId:guid}")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a book cover image.")]
    [EndpointDescription("Returns the cover image for the specified book. " +
                         "When the stored cover is an externally retrieved URL (e.g. Open Library or Google Books) " +
                         "the response is a redirect to that URL. For locally uploaded images the raw file bytes are returned.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBookImage")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    [ResponseCache(Duration = ApiConstants.ImagesResponseCacheDurationInSeconds, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult> GetBookImage([FromRoute] Guid bookId)
    {
        var fileNameResult = await sender.Send(new GetBookCoverPhotoFileNameByIdQuery(bookId));

        if (fileNameResult.IsFailure)
            return Problem(fileNameResult.Errors, HttpContext);

        // ── External cover URL (auto-retrieved from Open Library / Google Books) ─
        // The stored value is a fully-qualified https:// URL, not a local file name.
        // Redirect the client directly to the external source so we never attempt
        // to read it from the local file system (which would always return null).
        if (Uri.IsWellFormedUriString(fileNameResult.Value, UriKind.Absolute))
            return Redirect(fileNameResult.Value);

        // ── Local file (uploaded or default cover) ─────────────────────────────
        var extension = Path.GetExtension(fileNameResult.Value).ToLower();

        var image = await bookImageService.GetImage(fileNameResult.Value);

        if (image == null)
            return NotFound();

        return File(image, ImageHelper.GetContentType(extension));
    }

}
