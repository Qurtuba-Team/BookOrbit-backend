namespace BookOrbit.Application.Features.Identity.Commands.SendEmailConfirmation;
public class SendEmailConfirmationCommandHandler(
    IEmailConfirmationService emailConfirmationService,
    ILogger<SendEmailConfirmationCommandHandler> logger,
    IEmailService emailService,
    IRouteService routeService,
    IEmailFormatService emailFormatService) : IRequestHandler<SendEmailConfirmationCommand, Result<Success>>
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



        var linkResult = routeService.GetEmailConfirmationRoute(encodedConfirmationTokenResult.Value.Email, encodedConfirmationTokenResult.Value.EncodedConfirmationToken);

        var emailFormatResult = emailFormatService.ConfirmEmailFormat(linkResult);

        if (emailFormatResult.IsFailure)
        {
            logger.LogWarning("Email format generation failed for email {Email}: {Errors}",
                command.Email,
                emailFormatResult.Errors);
            return emailFormatResult.Errors;
        }


        var emailResult = await emailService.SendEmailAsync(
   encodedConfirmationTokenResult.Value.Email,
   "Confirm your email",
   emailFormatResult.Value);

        if (emailResult.IsFailure)
            return emailResult.Errors;

        return Result.Success;
    }
}
