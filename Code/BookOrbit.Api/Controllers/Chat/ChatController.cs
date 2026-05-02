namespace BookOrbit.Api.Controllers.Chat;

[Route("api/v{version:apiVersion}/chat")]
[ApiVersion("1.0")]
[Authorize]
public class ChatController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{

    [HttpPost("messages")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Send a chat message to another student.")]
    [EndpointDescription("Sends a text message to the specified student, creating a chat group between the two students if one does not already exist.")]
    [EndpointName("SendMessage")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var command = new SendMessageCommand(
            request.ReceiverId,
            request.Content);

        var result = await sender.Send(command, ct);

        return result.Match(
           messageDto => StatusCode(StatusCodes.Status201Created, messageDto),
           e => Problem(e, HttpContext));
    }


    [HttpPatch("groups/{chatGroupId:guid}/read")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Mark messages as read in a chat group.")]
    [EndpointDescription("Marks all unread messages sent by the other participant in the specified chat group as read for the current student.")]
    [EndpointName("MarkMessagesAsRead")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> MarkMessagesAsRead([FromRoute] Guid chatGroupId, CancellationToken ct)
    {
        var result = await sender.Send(new MarkMessagesAsReadCommand(chatGroupId), ct);

        return result.Match(
            _ => NoContent(),
            e => Problem(e, HttpContext));
    }


    [HttpGet("groups/{chatGroupId:guid}/messages")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve paginated chat history for a chat group.")]
    [EndpointDescription("Returns a paginated collection of messages for the specified chat group, ordered by most recent first. The current student must be a participant in the chat group.")]
    [EndpointName("GetChatHistory")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<ChatMessageDto>>> GetChatHistory(
        [FromRoute] Guid chatGroupId,
        [FromQuery] ChatPagedFilterRequest request,
        CancellationToken ct)
    {
        var query = new GetChatHistoryQuery(
            chatGroupId,
            request.Page,
            request.PageSize);

        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }


    [HttpGet("groups")]
    [Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
    [ProducesResponseType(typeof(PaginatedList<ChatGroupListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Retrieve the current student's chat groups.")]
    [EndpointDescription("Returns a paginated list of all chat groups the current student participates in, including the name and photo of the other participant.")]
    [EndpointName("GetUserChatGroups")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<PaginatedList<ChatGroupListItemDto>>> GetUserChatGroups(
        [FromQuery] ChatPagedFilterRequest request,
        CancellationToken ct)
    {
        var studentResult = await sender.Send(new GetStudentByUserIdQuery(currentUser.Id), ct);

        if (studentResult.IsFailure)
        {
            return Problem(studentResult.Errors, HttpContext);
        }

        var query = new GetUserChatGroupsQuery(
            studentResult.Value.Id,
            request.Page,
            request.PageSize);

        var result = await sender.Send(query, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }



}
