namespace BookOrbit.Application.Features.Identity.Commands.ChangePassword;
public class ChangePasswordCommandHandler(
    IPasswordService passwordService,
    ILogger<ChangePasswordCommandHandler> logger) : IRequestHandler<ChangePasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ChangePasswordCommand command, CancellationToken ct)
    {
        var result = await passwordService.ChangePasswordAsync(command.Email,command.OldPassword, command.NewPassword);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "Password change failed for user {Email}: {Errors}",
                command.Email,
                result.Errors);

            return result.Errors;
        }

        return Result.Success;
    }
}