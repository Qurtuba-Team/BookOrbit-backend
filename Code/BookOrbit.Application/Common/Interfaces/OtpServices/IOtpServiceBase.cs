namespace BookOrbit.Application.Common.Interfaces.OtpServices;
public interface IOtpServiceBase
{
    Task<Result<string>> GenerateOtp(Guid TargetId,DateTimeOffset CurrentTime,CancellationToken ct);
    Task<Result<Success>> VerifyOtp(string otp,Guid TargetId, DateTimeOffset CurrentTime, CancellationToken ct);
}