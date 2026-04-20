
namespace BookOrbit.Application.Features.Identity.Commands.ConfirmEmail;
public class ConfirmEmailCommandHandler(
    IEmailConfirmationService emailConfirmationService,
    ILogger<ConfirmEmailCommandHandler> logger) : IRequestHandler<ConfirmEmailCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        ConfirmEmailCommand request,
        CancellationToken ct)
    {
        var result = await emailConfirmationService.ConfirmEmailAsync(
            request.Email,
            request.EncodedToken,
            ct);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "Email confirmation failed for user {Email}: {Errors}",
                request.Email,
                result.Errors);

            return result.Errors;
        }

        return Result.Updated;
    }
}
