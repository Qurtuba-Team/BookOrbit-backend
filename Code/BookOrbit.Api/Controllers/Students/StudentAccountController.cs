namespace BookOrbit.Api.Controllers.Students;

[Route("api/v{version:apiVersion}/students")]
[ApiVersion("1.0")]
[Authorize]
public class StudentAccountController(
    ISender sender,
    ICurrentUser currentUser,
    ImageHelper imageHelper) : ApiController
{
    [HttpGet("me")]
    [Authorize(Policy = PoliciesNames.StudentOnlyPolicy)]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve the current authenticated student's profile.")]
    [EndpointDescription("Returns the student record that is linked to the currently authenticated user, including the details needed for the student's account experience.")]
    [EndpointName("GetCurrentStudent")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<StudentDto>> GetCurrentStudent(CancellationToken ct)
    {
        var userId = currentUser.Id;

        var query = new GetStudentByUserIdQuery(userId);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }


    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Create a new student account and student profile.")]
    [EndpointDescription("Registers a new student, stores the submitted profile information, and creates the linked user account required for authentication and future access to the platform.")]
    [EndpointName("CreateStudentAccount")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.SensitiveRateLimmitingPolicyName)]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromForm] CreateStudentRequest request, CancellationToken ct)
    {
        //Upload Image, Get Image Name 
        var ImageUploadResult = await imageHelper.UploadImage(request.PersonalPhoto, ImageHelper.StudentImagesUploadFolderPath);

        if (ImageUploadResult.IsFailure)
            return Problem(ImageUploadResult.Errors, HttpContext);

        var command = new CreateStudentCommand(
            request.Name,
            request.UniversityMailAddress,
            ImageUploadResult.Value,
            request.Password,
            request.PhoneNumber,
            request.TelegramUserId);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
             imageHelper.DeleteImage(ImageUploadResult.Value, ImageHelper.StudentImagesUploadFolderPath);


        return result.Match(
           studentDto => CreatedAtRoute(
               routeName: "GetStudentById",
               routeValues: new { version = "1.0", id = studentDto.Id },
               value: studentDto),

           e => Problem(e, HttpContext));
    }


    [HttpPatch("{id:guid}")]
    [Authorize(Policy = PoliciesNames.StudentOwnershipPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Update an existing student's profile information.")]
    [EndpointDescription("Updates the editable profile information for the specified student, including the uploaded personal photo when a new file is provided in the request.")]
    [EndpointName("UpdateStudent")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.SensitiveRateLimmitingPolicyName)]
    public async Task<ActionResult> UpdateStudent([FromRoute] Guid id, [FromForm] UpdateStudentRequest request, CancellationToken ct)
    {
        var imageFileNameResult = await sender.Send(new GetStudentPersonalPhotoFileNameByIdQuery(id),ct);

        if(imageFileNameResult.IsFailure)
            return Problem(imageFileNameResult.Errors, HttpContext);

        var imageFileName = imageFileNameResult.Value;

        if(request.PersonalPhoto is not null)
        {
            //Upload Image, Get Image Name
            var ImageUploadResult = await imageHelper.UploadImage(request.PersonalPhoto, ImageHelper.StudentImagesUploadFolderPath);

            if (ImageUploadResult.IsFailure)
                return Problem(ImageUploadResult.Errors, HttpContext);

            imageFileName = ImageUploadResult.Value;
        }


        var result = await sender.Send(
            new UpdateStudentCommand(
                id,
            request.Name,
            imageFileName),
            ct);

        if(result.IsSuccess)
        {
            if(request.PersonalPhoto is not null)
                //Delete Old Image
                 imageHelper.DeleteImage(imageFileNameResult.Value, ImageHelper.StudentImagesUploadFolderPath);

            return NoContent();
        }
        else
        {
            if (request.PersonalPhoto is not null)
                //Delete New Image
                 imageHelper.DeleteImage(imageFileName, ImageHelper.StudentImagesUploadFolderPath);

            return Problem(result.Errors, HttpContext);
        }
    }
}
