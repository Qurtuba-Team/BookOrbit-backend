namespace BookOrbit.Application.Features.Identity.Dtos;

public record TokenDto
{
    public string? AccessToken { get; set; } = null;
    public string? RefreshToken { get; set; } = null;
    public DateTime ExpiresOnUtc { get; set; } = default;
}