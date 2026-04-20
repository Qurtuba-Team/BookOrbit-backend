namespace BookOrbit.Api.Contracts.Requests.Identity;

public record ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string EncodedToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
