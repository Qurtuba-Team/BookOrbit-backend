namespace BookOrbit.Application.Features.Identity.Commands.GeneratreEmailConfirmationToken;


public class GenerateEmailConfirmationTokenCommandHandler(
    IIdentityService identityService,
    ILogger<GenerateEmailConfirmationTokenCommandHandler> logger) :
    IRequestHandler<GenerateEmailConfirmationTokenCommand, Result<EmailConfirmationTokenDto>>
{
    public async Task<Result<EmailConfirmationTokenDto>> Handle(
        GenerateEmailConfirmationTokenCommand request, 
        CancellationToken ct)
    {

        var encodedConfirmationTokenResult = await identityService.GenerateEmailConfirmationTokenAsync(request.UserId,ct);


        if (encodedConfirmationTokenResult.IsFailure)
        {
            logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}",
                request.UserId,
                encodedConfirmationTokenResult.Errors);
            return encodedConfirmationTokenResult.Errors;
        }

        return encodedConfirmationTokenResult.Value;
    }
}
