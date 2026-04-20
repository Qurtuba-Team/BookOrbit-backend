namespace BookOrbit.Application.Features.Identity.Commands.SendResetPassword;
public class SendResetPasswordCommandHandler(
    IPasswordService passwordService,
    ILogger<SendResetPasswordCommandHandler> logger,
    IRouteService routeService,
    IEmailFormatService emailFormatService,
    IEmailService emailService) : IRequestHandler<SendResetPasswordCommand, Result<Success>>

{
    public async Task<Result<Success>> Handle(SendResetPasswordCommand command, CancellationToken ct)
    {
        var encodedConfirmationTokenResult = await passwordService.GenerateResetPasswordTokenAsync(command.Email, ct);

        if (encodedConfirmationTokenResult.IsFailure)
        {
            logger.LogWarning("Password reset failed for email {Email}: {Errors}",
                command.Email,
                encodedConfirmationTokenResult.Errors);
            return encodedConfirmationTokenResult.Errors;
        }


        var linkResult = routeService.GetResetPasswordRoute(encodedConfirmationTokenResult.Value.Email, encodedConfirmationTokenResult.Value.EncoddedToken);

        var emailFormatResult = emailFormatService.ResetPasswordEmailFormat(linkResult);

        if (emailFormatResult.IsFailure)
        {
            logger.LogWarning("Password reset email format generation failed for email {Email}: {Errors}",
                command.Email,
                emailFormatResult.Errors);
            return emailFormatResult.Errors;
        }


        var emailResult = await emailService.SendEmailAsync(
   encodedConfirmationTokenResult.Value.Email,
   "Reset your password",
   emailFormatResult.Value);

        if (emailResult.IsFailure)
            return emailResult.Errors;

        return Result.Success;
    }
}