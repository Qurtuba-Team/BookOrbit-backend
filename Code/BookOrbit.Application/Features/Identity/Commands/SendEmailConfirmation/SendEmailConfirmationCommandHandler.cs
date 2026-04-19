namespace BookOrbit.Application.Features.Identity.Commands.SendEmailConfirmation;
public class SendEmailConfirmationCommandHandler(
    IEmailConfirmationService emailConfirmationService,
    ILogger<SendEmailConfirmationCommandHandler> logger,
    IRouteService routeService,
    IEmailService emailService) : IRequestHandler<SendEmailConfirmationCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(SendEmailConfirmationCommand command, CancellationToken ct)
    {
        var encodedConfirmationTokenResult = await emailConfirmationService.GenerateEmailConfirmationTokenAsync(command.Email, ct);

        if (encodedConfirmationTokenResult.IsFailure)
        {
            logger.LogWarning("Email confirmation failed for email {Email}: {Errors}",
                command.Email,
                encodedConfirmationTokenResult.Errors);
            return encodedConfirmationTokenResult.Errors;
        }


        var linkResult = routeService.GetRouteByName("ConfirmEmail",
        new { email = command.Email, token = encodedConfirmationTokenResult.Value.EncodedConfirmationToken, version = "1.0" });

        if (linkResult.IsFailure)
        {
            logger.LogError("Failed to generate email confirmation link for email {Email}: {Errors}",
                command.Email,
                linkResult.Errors);
            return linkResult.Errors;
        }

        await emailService.SendEmailAsync(
            encodedConfirmationTokenResult.Value.Email,
            "Confirm your email",
            $"Click here: <a href='{linkResult.Value}'>Confirm</a>");

        return Result.Success;
    }
}
