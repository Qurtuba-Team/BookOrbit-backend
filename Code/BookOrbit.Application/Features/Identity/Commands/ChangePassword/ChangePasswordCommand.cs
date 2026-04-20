namespace BookOrbit.Application.Features.Identity.Commands.ChangePassword;
public record ChangePasswordCommand(
    string Email,
    string OldPassword,
    string NewPassword) : IRequest<Result<Success>>;