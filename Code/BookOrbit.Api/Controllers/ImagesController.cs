

using BookOrbit.Application.Common.Interfaces.ImageServices;

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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a book cover image.")]
    [EndpointDescription("Returns the cover image file associated with the specified book so clients can display the book artwork in listings and detail views.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetBookImage")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    [ResponseCache(Duration = ApiConstants.ImagesResponseCacheDurationInSeconds, Location = ResponseCacheLocation.Client)] // Store in browser
    public async Task<ActionResult> GetBookImage([FromRoute] Guid bookId)
    {
        var fileNameResult = await sender.Send(new GetBookCoverPhotoFileNameByIdQuery(
            bookId));

        if (fileNameResult.IsFailure)
            return Problem(fileNameResult.Errors, HttpContext);


        var extension = Path.GetExtension(fileNameResult.Value).ToLower();

        var image = await bookImageService.GetImage(fileNameResult.Value);

        if (image == null)
            return NotFound();


        return File(image, ImageHelper.GetContentType(extension));
    }

}
