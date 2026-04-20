namespace BookOrbit.Application.Features.Identity.Commands.ResetPassowrd;
public record ResetPasswordCommand(
    string Email,
    string EncodedToken,
    string NewPassword) : IRequest<Result<Updated>>;