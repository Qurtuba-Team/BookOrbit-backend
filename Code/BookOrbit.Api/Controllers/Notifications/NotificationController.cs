namespace BookOrbit.Api.Controllers.Notifications;

[Route("api/v{version:apiVersion}/notifications")]
[ApiVersion("1.0")]
[Authorize]
public class NotificationController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    [HttpPatch("read")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Mark notifications as read.")]
    [EndpointDescription("Marks all unread notifications for the current student as read up to the specified cutoff time.")]
    [EndpointName("MarkNotificationsAsRead")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> MarkNotificationsAsRead([FromBody] MarkNotificationAsReadRequest request, CancellationToken ct)
    {
        var studentResult = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentResult.IsFailure)
        {
            return Problem(studentResult.Errors, HttpContext);
        }

        var result = await sender.Send(new MarkAsReadCommand(studentResult.Value.Id, request.MaxTime), ct);

        return result.Match(
            _ => NoContent(),
            e => Problem(e, HttpContext));
    }

    [HttpGet("{notificationId:guid}", Name = "GetNotificationById")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a notification by identifier.")]
    [EndpointDescription("Returns the notification details for the specified identifier when the notification exists for the current student.")]
    [EndpointName("GetNotificationById")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<NotificationDto>> GetNotificationById([FromRoute] Guid notificationId, CancellationToken ct)
    {
        var studentResult = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentResult.IsFailure)
        {
            return Problem(studentResult.Errors, HttpContext);
        }

        var query = new GetNotificationByIdQuery(studentResult.Value.Id, notificationId);
        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }

    [HttpGet]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<NotificationListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve a paginated list of notifications.")]
    [EndpointDescription("Returns a paginated collection of notifications for the current student and supports searching, filtering, and sorting by [createdat,updatedat,title,type,isread].")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetNotifications")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<NotificationListItemDto>>> GetNotifications([FromQuery] NotificationPagedFilterRequest request, CancellationToken ct)
    {
        var studentResult = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentResult.IsFailure)
        {
            return Problem(studentResult.Errors, HttpContext);
        }

        var query = new GetNotificationsQuery(
            studentResult.Value.Id,
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortColumn,
            request.SortDirection,
            request.IsRead,
            request.Types);

        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }
}
