namespace BookOrbit.Application.Features.Identity.Dtos;

public record AppUserDto(
    string UserId,
    string Email, 
    IList<string> Roles,
    IList<Claim> Claims,
    bool EmailConfirmed);