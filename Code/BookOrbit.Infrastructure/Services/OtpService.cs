using BookOrbit.Application.Common.OTPs;
using BookOrbit.Domain.Otps;

namespace BookOrbit.Infrastructure.Services;
public class OtpService(
    ILogger<OtpService> logger,
    IAppDbContext context,
    TimeProvider timeProvider) : IOtpService
{
    static private string GenerateOtpCode()
    {
        Random rnd = new();
        return rnd.Next(100000, 999999).ToString();
    }

    public async Task<Result<string>> GenerateBorrowingRequestOtp(Guid BorrowingRequestId, CancellationToken ct)
    {
        var BorrowingRequestData = await context.
            BorrowingRequests
            .AsNoTracking()
            .Select(br => new
            {
                br.Id,
                br.State
            })
            .FirstOrDefaultAsync(
            br => br.Id == BorrowingRequestId, ct);


        if (BorrowingRequestData!.State is not BorrowingRequestState.Accepted)
        {
            logger.LogWarning("Cannot send an otp to a request that hasnt been accepted yet {RequestId}", BorrowingRequestData.Id);
            return BorrowingRequestApplicationErrors.BorrowingRequestNotAccepted;
        }

        var otpCode = GenerateOtpCode();

        var otpCreationResult = Otp.Create(BorrowingRequestId, otpCode, Domain.Otps.Enums.OtpType.BorrowingRequestDelivery, timeProvider.GetUtcNow());

        if (otpCreationResult.IsFailure)
        {
            logger.LogWarning("otp Creation Faild For Borrowing Request Id {BorroiwngRequestId}", BorrowingRequestId);
        }

        context.Otps.Add(otpCreationResult.Value);
        await context.SaveChangesAsync(ct);

        return otpCode;
    }

    public async Task<Result<Success>> VerifyBorrwoingReuqestOtp(string otp, Guid BorrowingRequestId, CancellationToken ct)
    {

        var BorrowingRequestData = await context.
            BorrowingRequests
            .AsNoTracking()
            .Select(br => new
            {
                br.Id,
                br.State
            })
            .FirstOrDefaultAsync(
            br => br.Id == BorrowingRequestId, ct);


        if (BorrowingRequestData!.State is not BorrowingRequestState.Accepted)
        {
            logger.LogWarning("Cannot send an otp to a request that hasnt been accepted yet {RequestId}", BorrowingRequestData.Id);
            return BorrowingRequestApplicationErrors.BorrowingRequestNotAccepted;
        }


        var otpData = await context.Otps.AsNoTracking()
            .Where(o => o.TargetId == BorrowingRequestId
            && o.Type == Domain.Otps.Enums.OtpType.BorrowingRequestDelivery)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if(otpData is null)
        {
            logger.LogWarning("No otp found for Borrowing Request Id {BorrowingRequestId}", BorrowingRequestId);
            return OtpApplicationErrors.OtpNotFound;
        }

        if (otpData.ExpirationDateUtc < timeProvider.GetUtcNow())
        {
            logger.LogWarning("Otp expired for Borrowing Request Id {BorrowingRequestId}", BorrowingRequestId);
            return OtpApplicationErrors.OtpExpired;
        }

        if (otpData.Code != otp)
        {
            logger.LogWarning("Invalid otp for Borrowing Request Id {BorrowingRequestId}", BorrowingRequestId);
            return OtpApplicationErrors.OtpInvalid;
        }

        return Result.Success;
    }
}