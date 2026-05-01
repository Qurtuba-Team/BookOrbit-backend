
namespace BookOrbit.Infrastructure.Services.OtpServices;
public class BorrowingRequestOtpService(
    ILogger<BorrowingRequestOtpService> logger,
    IAppDbContext context) : OtpServiceBase(logger), IBorrowingRequestOtpService
{
    protected override async Task<Result<Success>> ValidateTarget(Guid targetId, CancellationToken ct)
    {
        var data = await context.BorrowingRequests
            .AsNoTracking()
            .Where(x => x.Id == targetId)
            .Select(x => new { x.State })
            .FirstOrDefaultAsync(ct);

        if(data is null)
        {
            return BorrowingRequestApplicationErrors.NotFoundById;
        }

        if (data.State != BorrowingRequestState.Accepted)
        {
            return BorrowingRequestApplicationErrors.BorrowingRequestNotAccepted;
        }

        return Result.Success;
    }

    protected override Result<Otp> CreateOtp(Guid targetId, string code, DateTimeOffset currentTime)
    {
        return Otp.Create(
            Guid.NewGuid(),
            targetId,
            code,
            OtpType.BorrowingRequestDelivery,
            currentTime
        ).Value;
    }

    protected override async Task<Result<Success>> SaveOtp(Otp otp, CancellationToken ct)
    {
        context.Otps.Add(otp);
        await context.SaveChangesAsync(ct);
        return Result.Success;
    }

    protected override async Task<Otp?> GetLatestOtp(Guid targetId, CancellationToken ct)
    {
        return await context.Otps
            .AsNoTracking()
            .Where(o => o.TargetId == targetId &&
                        o.Type == OtpType.BorrowingRequestDelivery)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
    }

    protected override async Task<Result<Success>> MarkOtpAsUsed(Otp otp, CancellationToken ct)
    {
        otp.Use(DateTime.UtcNow);
        context.Otps.Update(otp);
        await context.SaveChangesAsync(ct);
        return Result.Success;
    }
}