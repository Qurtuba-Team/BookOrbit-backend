namespace BookOrbit.Api.Controllers.Students;

[Route("api/v{version:apiVersion}/students")]
[ApiVersion("1.0")]

[EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
[Authorize(Policy = PoliciesNames.AdminOnlyPolicy)]

[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]

public class StudentManagementController(
    ISender sender) : ApiController
{
    [HttpPatch("{studentId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Approve a pending student account.")]
    [EndpointDescription("Approves the specified student after the required validation checks pass so the student can move forward in the account lifecycle.")]
    [EndpointName("ApproveStudent")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> ApproveStudent([FromRoute] Guid studentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new ApproveStudentCommand(
                studentId),
            ct);

        return result.Match(
            response => NoContent(),
            e => Problem(e, HttpContext));
    }



    [HttpPatch("{studentId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Activate an approved student account.")]
    [EndpointDescription("Marks the specified student as active after approval so the student can use the platform according to the assigned permissions.")]
    [EndpointName("ActivateStudent")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> ActivateStudent([FromRoute] Guid studentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new ActivateStudentCommand(
                studentId),
            ct);

        return result.Match(
            response => NoContent(),
            e => Problem(e, HttpContext));
    }


    [HttpPatch("{studentId:guid}/ban")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Ban a student account.")]
    [EndpointDescription("Blocks the specified student from using the platform by moving the account into a banned state until further administrative action is taken.")]
    [EndpointName("BanStudent")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> BanStudent([FromRoute] Guid studentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new BanStudentCommand(
                studentId),
            ct);

        return result.Match(
            response => NoContent(),
            e => Problem(e, HttpContext));
    }


    [HttpPatch("{studentId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Reject a student account request.")]
    [EndpointDescription("Rejects the specified student account while it is under review so it does not proceed to activation or normal platform use.")]
    [EndpointName("RejectStudent")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> RejectStudent([FromRoute] Guid studentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new RejectStudentCommand(
                studentId),
            ct);

        return result.Match(
            response => NoContent(),
            e => Problem(e, HttpContext));
    }


    [HttpPatch("{studentId:guid}/unban")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Remove the ban from a student account.")]
    [EndpointDescription("Restores access for the specified student by removing the banned state so the account can continue through the expected lifecycle.")]
    [EndpointName("UnbanStudent")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> UnbanStudent([FromRoute] Guid studentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new UnBanStudentCommand(
                studentId),
            ct);

        return result.Match(
            response => NoContent(),
            e => Problem(e, HttpContext));
    }


    [HttpPatch("{studentId:guid}/pend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Move a student account back to the pending state.")]
    [EndpointDescription("Returns the specified student account to the pending review state, which is useful when an earlier rejection needs to be reconsidered.")]
    [EndpointName("PendStudent")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> PendStudent([FromRoute] Guid studentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new PendStudentCommand(
                studentId),
            ct);

        return result.Match(
            response => NoContent(),
            e => Problem(e, HttpContext));
    }

}
