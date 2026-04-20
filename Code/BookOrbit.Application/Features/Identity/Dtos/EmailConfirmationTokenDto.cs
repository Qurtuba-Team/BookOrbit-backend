namespace BookOrbit.Application.Features.Identity.Dtos;
public record EmailConfirmationTokenDto(
    string EncodedConfirmationToken,
    string Email);