namespace BookOrbit.Application.Common.Interfaces;
public interface IOtpService
{
    Task<Result<string>> GenerateBorrowingRequestOtp(Guid BorrowingRequestId,CancellationToken ct);
    Task<Result<Success>> VerifyBorrwoingReuqestOtp(string otp,Guid BorrowingRequestId,CancellationToken ct);
}