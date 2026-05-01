namespace BookOrbit.Api.Contracts.Requests.Otps;
public record OtpRequest
{
    public string OtpCode { get; set; } = string.Empty;
}