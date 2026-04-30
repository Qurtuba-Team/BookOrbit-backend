namespace BookOrbit.Application.Common.Interfaces;
public interface IOtpService
{
    Task<Result<string>> GenerateOtp(string email,Guid BorrowingRequestId,CancellationToken ct);
    Task<Result<Success>> VerifyOtp(string email, string otp,Guid BorroiwngRequestId,CancellationToken ct);
}