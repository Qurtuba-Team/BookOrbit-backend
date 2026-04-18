using BookOrbit.Application.Features.Onboarding;
using BookOrbit.Application.Features.Onboarding.Commands.CompleteOnboarding;
using BookOrbit.Application.Features.Onboarding.Queries.GetAvailableInterests;
using BookOrbit.Api.Contracts.Requests.Onboarding;

namespace BookOrbit.Api.Controllers.Onboarding;

[Route("api/v{version:apiVersion}/onboarding")]
[ApiVersion("1.0")]
[Authorize(Policy = PoliciesNames.RegisteredUserPolicy)]
public class OnboardingController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    [HttpGet("interests")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<InterestDto>), StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get all available interests for onboarding.")]
    [EndpointDescription("Returns the full list of seeded interest options a user can choose from during onboarding.")]
    [EndpointName("GetAvailableInterests")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<List<InterestDto>>> GetAvailableInterests(CancellationToken ct)
    {
        var result = await sender.Send(new GetAvailableInterestsQuery(), ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Complete onboarding for the authenticated user.")]
    [EndpointDescription("Collects the user's academic year, faculty, and interests (1–5) so the recommendation engine can personalise book suggestions.")]
    [EndpointName("CompleteOnboarding")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> CompleteOnboarding(
        [FromBody] CompleteOnboardingRequest request,
        CancellationToken ct)
    {
        var command = request.ToCommand(currentUser.Id!);
        var result = await sender.Send(command, ct);

        if (result.IsFailure && result.TopError.Code == OnboardingErrors.AlreadyCompleted.Code)
            return Conflict(new { message = OnboardingErrors.AlreadyCompleted.Description });

        return result.Match(
            _ => Ok(),
            e => Problem(e, HttpContext));
    }
}
