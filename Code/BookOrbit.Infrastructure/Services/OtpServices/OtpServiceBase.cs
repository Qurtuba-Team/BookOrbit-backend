using BookOrbit.Application.Features.OTPs;

namespace BookOrbit.Infrastructure.Services.OtpServices;
public abstract class OtpServiceBase(ILogger logger) : IOtpServiceBase
{
    protected static string GenerateOtpCode()
    {
        Random rnd = new();
        return rnd.Next(100000, 999999).ToString();
    }

    public async Task<Result<string>> GenerateOtp(Guid targetId, DateTimeOffset currentTime, CancellationToken ct)
    {
        var validationResult = await ValidateTarget(targetId, ct);
        if (validationResult.IsFailure)
            return validationResult.Errors;

        var code = GenerateOtpCode();

        var otpResult = CreateOtp(targetId, code, currentTime);

        if (otpResult.IsFailure)
        {
            logger.LogWarning("OTP creation failed for {TargetId}", targetId);
            return otpResult.Errors;
        }

        var saveOtpResult = await SaveOtp(otpResult.Value, ct);

        if (saveOtpResult.IsFailure)
        {
            logger.LogWarning("Saving OTP failed for {TargetId}", targetId);
            return saveOtpResult.Errors;
        }

        return code;
    }

    public async Task<Result<Success>> VerifyOtp(string otp, Guid targetId,DateTimeOffset currentTime, CancellationToken ct)
    {
        var validationResult = await ValidateTarget(targetId, ct);
        if (validationResult.IsFailure)
            return validationResult.Errors;

        var otpData = await GetLatestOtp(targetId, ct);

        if (otpData is null)
            return OtpApplicationErrors.OtpInvalid;

        if (IsExpired(otpData, currentTime))
            return OtpApplicationErrors.OtpInvalid;

        if (!IsValid(otpData, otp))
            return OtpApplicationErrors.OtpInvalid;

        if(otpData.IsUsed)
            return OtpApplicationErrors.OtpInvalid;

        var markOtpAsUsedResult = await MarkOtpAsUsed(otpData, ct);

        if (markOtpAsUsedResult.IsFailure)
        {
            logger.LogWarning("Marking OTP as used failed for {TargetId}", targetId);
            return markOtpAsUsedResult.Errors;
        }

        return Result.Success;
    }

    protected abstract Task<Result<Success>> ValidateTarget(Guid targetId, CancellationToken ct);
    protected abstract Task<Result<Success>> SaveOtp(Otp otp, CancellationToken ct);
    protected abstract Result<Otp> CreateOtp(Guid targetId, string code, DateTimeOffset currentTime);
    protected abstract Task<Otp?> GetLatestOtp(Guid targetId, CancellationToken ct);
    protected abstract Task<Result<Success>> MarkOtpAsUsed(Otp otp, CancellationToken ct);

    protected virtual bool IsExpired(Otp otp, DateTimeOffset currentTime)
        => otp.ExpirationDateUtc < currentTime;

    protected virtual bool IsValid(Otp otp, string input)
        => otp.Code == input;
}