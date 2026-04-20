namespace BookOrbit.Api.Contracts.Requests.Identity;

public record ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
