namespace BookOrbit.Api.Controllers.Students;

[Route("api/v{version:apiVersion}/students")]
[ApiVersion("1.0")]
[Authorize]
public class StudentQueryController(
    ISender sender) : ApiController
{
    [HttpGet]
    [Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]
    [ProducesResponseType(typeof(PaginatedList<StudentListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of students.")]
    [EndpointDescription("Returns a paginated collection of students and supports filtering, free-text searching, and sorting by [joindate,state,name,updatedat,createdat] asc / descso administrators can review student records efficiently.")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetStudents")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<StudentListItemDto>>> GetStudents([FromQuery] StudentPagedFilterRequest request, CancellationToken ct)
    {
        var query = new GetStudentsQuery(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.States,
            request.EmailConfirmed);

        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }



    [HttpGet("{studentId:guid}", Name = "GetStudentById")]
    [Authorize(Policy = PoliciesNames.StudentOwnershipPolicy)]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a student by identifier.")]
    [EndpointDescription("Returns the full student details for the specified student identifier when the student record exists.")]
    [EndpointName("GetStudentById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = ApiConstants.DefaultOutputCachePolicyName)]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<StudentDto>> GetStudentById([FromRoute] Guid studentId, CancellationToken ct)
    {
        var query = new GetStudentByIdQuery(studentId);

        var result = await sender.Send(query, ct);

        return result.Match(
           Ok,
           e => Problem(e, HttpContext));
    }

}
