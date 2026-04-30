
namespace BookOrbit.Api.Controllers.Chatbot;

[Route("api/v{version:apiVersion}/chatbot")]
[ApiVersion("1.0")]
[Authorize(Policy = PoliciesNames.ActiveStudentPolicy)]
public class ChatbotController(ISender sender) : ApiController
{
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ChatbotResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Send a message to the BookOrbit chatbot.")]
    [EndpointDescription("Sends a natural-language message to the AI chatbot. The chatbot may search books, check points, or provide information about the platform. Conversation history is maintained server-side per user session.")]
    [EndpointName("SendChatMessage")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.SensitiveRateLimitingPolicyName)]
    public async Task<ActionResult<ChatbotResponseDto>> SendMessage(
        [FromBody] SendChatMessageRequest request,
        CancellationToken ct)
    {
        var command = new SendChatMessageCommand(request.Message);
        var result = await sender.Send(command, ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }
}
