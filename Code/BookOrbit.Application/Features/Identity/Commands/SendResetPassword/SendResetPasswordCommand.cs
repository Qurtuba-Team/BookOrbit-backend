namespace BookOrbit.Application.Features.Identity.Commands.SendResetPassword;
public record SendResetPasswordCommand(
    string Email) : IRequest<Result<Success>>;