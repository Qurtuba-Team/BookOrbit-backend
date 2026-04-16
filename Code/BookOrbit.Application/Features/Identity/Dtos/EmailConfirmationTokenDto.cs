namespace BookOrbit.Application.Features.Identity.Dtos;
public record EmailConfirmationTokenDto(
    string encodedConfirmationToken,
    string email);