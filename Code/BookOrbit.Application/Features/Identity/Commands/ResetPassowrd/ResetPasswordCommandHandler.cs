namespace BookOrbit.Application.Features.Identity.Commands.ResetPassowrd;
public class ResetPasswordCommandHandler(
    IPasswordService passwordService,
    ILogger<ResetPasswordCommandHandler> logger) : IRequestHandler<ResetPasswordCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var result = await passwordService.ResetPasswordAsync(
    command.Email,
    command.EncodedToken,
    command.NewPassword,
    ct);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "Password reset failed for user {Email}: {Errors}",
                command.Email,
                result.Errors);

            return result.Errors;
        }

        return Result.Updated;
    }
}