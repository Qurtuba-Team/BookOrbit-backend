using BookOrbit.Application.Features.Recommendations.Commands.RecordInteraction;
using BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;
using BookOrbit.Application.Features.Recommendations.Queries.GetTrendingBooks;
using BookOrbit.Api.Contracts.Requests.Recommendations;

namespace BookOrbit.Api.Controllers.Recommendations;

[Route("api/v{version:apiVersion}/recommendations")]
[ApiVersion("1.0")]
[ApiController]
public class RecommendationsController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    [HttpGet]
    [Authorize(Policy = PoliciesNames.RegisteredUserPolicy)]
    [ProducesResponseType(typeof(List<RecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get personalised book recommendations for the authenticated user.")]
    [EndpointDescription("Returns a list of recommended books based on the user's academic year, faculty, and interests. Results are cached per-user for 6 hours.")]
    [EndpointName("GetRecommendations")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<List<RecommendationDto>>> GetRecommendations(CancellationToken ct)
    {
        var result = await sender.Send(new GetRecommendationsQuery(currentUser.Id!), ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<RecommendationDto>), StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Get globally trending books ordered by average rating.")]
    [EndpointDescription("Returns the top-rated books across all users. No authentication required.")]
    [EndpointName("GetTrendingBooks")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult<List<RecommendationDto>>> GetTrendingBooks(CancellationToken ct)
    {
        var result = await sender.Send(new GetTrendingBooksQuery(), ct);

        return result.Match(
            Ok,
            e => Problem(e, HttpContext));
    }

    [HttpPost("interact")]
    [Authorize(Policy = PoliciesNames.RegisteredUserPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [EndpointSummary("Record a user's interaction with a book.")]
    [EndpointDescription("Upserts the user's rating, read status, and wishlist status for the specified book. Invalidates the user's recommendation cache.")]
    [EndpointName("RecordBookInteraction")]
    [MapToApiVersion("1.0")]
    [EnableRateLimiting(ApiConstants.NormalRateLimitingPolicyName)]
    public async Task<ActionResult> RecordInteraction(
        [FromBody] RecordInteractionRequest request,
        CancellationToken ct)
    {
        var command = new RecordInteractionCommand(
            UserId: currentUser.Id!,
            BookId: request.BookId,
            Rating: request.Rating,
            IsRead: request.IsRead,
            IsWishlisted: request.IsWishlisted);

        var result = await sender.Send(command, ct);

        return result.Match(
            _ => Ok(),
            e => Problem(e, HttpContext));
    }
}
