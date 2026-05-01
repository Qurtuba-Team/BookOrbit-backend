
using BookOrbit.Application.Features.BorrowingTransactions;

namespace BookOrbit.Infrastructure.Services.OtpServices;
public class BorrowingTransactionOtpService(
    ILogger<BorrowingTransactionOtpService> logger,
    IAppDbContext context) : OtpServiceBase(logger), IBorrowingTransactionOtpService
{
    protected override Result<Otp> CreateOtp(Guid targetId, string code, DateTimeOffset currentTime)
    {
        return Otp.Create(
            Guid.NewGuid(),
            targetId,
            code,
            OtpType.BorrowingTransactionReturn,
            currentTime
        ).Value;
    }

    protected override async Task<Otp?> GetLatestOtp(Guid targetId, CancellationToken ct)
    {
        return await context.Otps
            .AsNoTracking()
            .Where(o => o.TargetId == targetId &&
                        o.Type == OtpType.BorrowingTransactionReturn)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
    }

    protected override async Task<Result<Success>> SaveOtp(Otp otp, CancellationToken ct)
    {
        context.Otps.Add(otp);
        await context.SaveChangesAsync(ct);
        return Result.Success;
    }

    protected override async Task<Result<Success>> ValidateTarget(Guid targetId, CancellationToken ct)
    {
        var data = await context.BorrowingTransactions
            .Where(bt => bt.Id == targetId)
            .Select(bt => new
            {
                bt.State
            })
            .FirstOrDefaultAsync(ct);

        if (data is null)
        {
            return BorrowingTransactionApplicationErrors.NotFoundById;
        }

        if(data.State is not BorrowingTransactionState.Borrowed)
        {
            return BorrowingTransactionApplicationErrors.InvalidState;
        }

        return Result.Success;
    }
    protected override async Task<Result<Success>> MarkOtpAsUsed(Otp otp, CancellationToken ct)
    {
        otp.Use(DateTime.UtcNow);
        context.Otps.Update(otp);
        await context.SaveChangesAsync(ct);
        return Result.Success;
    }

}