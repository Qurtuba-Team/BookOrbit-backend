namespace BookOrbit.Application.Features.Identity.Commands.GeneratreEmailConfirmationToken;

public record GenerateEmailConfirmationTokenCommand(
    string UserId):IRequest<Result<EmailConfirmationTokenDto>>;
