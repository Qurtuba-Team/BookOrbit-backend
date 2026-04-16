
namespace BookOrbit.Application.Features.Identity.Commands.ConfirmEmail;
public class ConfirmEmailCommandHandler(
    IIdentityService identityService,
    ILogger<ConfirmEmailCommandHandler> logger) : IRequestHandler<ConfirmEmailCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        ConfirmEmailCommand request,
        CancellationToken ct)
    {
        var result = await identityService.ConfirmEmailAsync(
            request.UserId,
            request.EncodedToken,
            ct);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "Email confirmation failed for user {UserId}: {Errors}",
                request.UserId,
                result.Errors);

            return result.Errors;
        }

        return Result.Updated;
    }
}
