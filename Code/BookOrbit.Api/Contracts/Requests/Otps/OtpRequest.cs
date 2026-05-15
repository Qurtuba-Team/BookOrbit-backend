namespace BookOrbit.Api.Contracts.Requests.Otps;
public record OtpRequest : ConcurrencyRequest
{
    public string OtpCode { get; set; } = string.Empty;
    public override string RowVersion { get; set; } = string.Empty;
}