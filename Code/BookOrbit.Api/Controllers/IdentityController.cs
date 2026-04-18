

namespace BookOrbit.Api.Controllers;

[Route("api/v{version:apiVersion}/identity")]
[ApiVersion("1.0")]
[ApiController]
public class IdentityController(ISender sender, IEmailService emailService,ICurrentUser currentUser) : ApiController
{
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Generate an access token and refresh token for a user.")]
    [EndpointDescription("Authenticates the user with the provided credentials and returns a token pair that can be used to access protected endpoints and renew the session.")]
    [EndpointName("GenerateToken")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.SensitiveRateLimmitingPolicyName)]
    public async Task<ActionResult<TokenDto>> GenerateToken([FromBody] GenerateTokenQuery request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);
        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }

    [HttpPost("token/refresh")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Refresh an authenticated session by issuing a new token pair.")]
    [EndpointDescription("Validates the submitted refresh token together with the expired access token context and returns a new access token and refresh token pair.")]
    [EndpointName("RefreshToken")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.SensitiveRateLimmitingPolicyName)]
    public async Task<ActionResult<TokenDto>> RefreshToken([FromBody] RefreshTokenQuery request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);
        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }


    [HttpGet("users/me")]
    [Authorize(Policy = PoliciesNames.RegisteredUserPolicy)]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesDefaultResponseType]
    [MapToApiVersion("1.0")]
    [EndpointSummary("Retrieve the currently authenticated user's account details.")]
    [EndpointDescription("Returns the application user profile associated with the access token that was used to call the endpoint.")]
    [EndpointName("GetCurrentUser")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<AppUserDto>> GetCurrentUser(CancellationToken ct)
    {
        var userId = currentUser.Id;

        var result = await sender.Send(
            new GetUserByIdQuery(userId),
            ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }



    [HttpPost("users/{userId}/send-email-confirmation")]
    [Authorize(Policy = PoliciesNames.RegisteredUserOwnershipPolicy)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesDefaultResponseType]
    [MapToApiVersion("1.0")]
    [EndpointSummary("Send an email confirmation link to a user account.")]
    [EndpointDescription("Generates an email confirmation token for the specified user account and sends a confirmation message that contains the verification link.")]
    [EndpointName("SendEmailConfirmation")]
    [EnableRateLimiting(ApiConstants.SensitiveRateLimmitingPolicyName)]
    public async Task<ActionResult> SendEmailConfirmation([FromRoute] string userId, CancellationToken ct)
    {
        var confirmationResult = await sender.Send(new GenerateEmailConfirmationTokenCommand(userId), ct);

        if (confirmationResult.IsFailure)
            return Problem(confirmationResult.Errors, HttpContext);

        var link = Url.RouteUrl("ConfirmEmail",
        new { Id = userId, token = confirmationResult.Value.encodedConfirmationToken, version = "1.0" },
        Request.Scheme);

        await emailService.SendEmailAsync(
            confirmationResult.Value.email,
            "Confirm your email",
            $"Click here: <a href='{link}'>Confirm</a>");

        return Ok("Email Sent");
    }


    [HttpGet("confirm-email", Name = "ConfirmEmail")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesDefaultResponseType]
    [MapToApiVersion("1.0")]
    [EndpointSummary("Confirm a user's email address.")]
    [EndpointDescription("Validates the confirmation token received from the email message and marks the user's email address as confirmed when the token is valid.")]
    [EndpointName("ConfirmEmail")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> ConfirmEmail(
    [FromQuery] string Id,
    [FromQuery] string token,
    CancellationToken ct)
    {
        var result = await sender.Send(new ConfirmEmailCommand(Id, token), ct);

        if (result.IsFailure)
            return Problem(result.Errors, HttpContext);

        return Ok("Email confirmed successfully");
    }



}
