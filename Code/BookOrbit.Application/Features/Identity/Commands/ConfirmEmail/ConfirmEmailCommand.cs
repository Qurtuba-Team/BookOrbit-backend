namespace BookOrbit.Application.Features.Identity.Commands.ConfirmEmail;
public record ConfirmEmailCommand(
    string Email,
    string EncodedToken):IRequest<Result<Updated>>;
