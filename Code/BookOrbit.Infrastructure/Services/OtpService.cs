using BookOrbit.Domain.Otps;

namespace BookOrbit.Infrastructure.Services;
public class OtpService(
    ILogger<OtpService>logger,
    IAppDbContext context,
    TimeProvider timeProvider) : IOtpService
{
    static private string GenerateOtpCode()
    {
        Random rnd = new ();
        return rnd.Next(100000, 999999).ToString();
    }

    public async Task<Result<string>> GenerateOtp(string email, Guid BorrowingRequestId, CancellationToken ct)
    {
        var studentExists = await context.Students.AsNoTracking()
            .AnyAsync(s => s.UniversityMail.Value == email,ct);

        if(!studentExists)
        {
            logger.LogWarning("Student with email {Email} not found.", email);
            return StudentApplicationErrors.NotFoundByEmail;
        }

        var BorrowingRequestData = await context.
            BorrowingRequests
            .AsNoTracking()
            .Select(br=>new
            {
                br.Id,
                br.State
            })
            .FirstOrDefaultAsync(
            br => br.Id == BorrowingRequestId, ct);
            
        
        if(BorrowingRequestData!.State is not BorrowingRequestState.Accepted)
        {
            logger.LogWarning("Cannot send an otp to a request that hasnt been accepted yet {RequestId}", BorrowingRequestData.Id);
            return BorrowingRequestApplicationErrors.BorrowingRequestNotAccepted;
        }

        var otpCode = GenerateOtpCode();

        var otpCreationResult = Otp.Create(BorrowingRequestId, otpCode, Domain.Otps.Enums.OtpType.BorrowingRequestDelivery,timeProvider.GetUtcNow());

        if(otpCreationResult.IsFailure)
        {
            logger.LogWarning("otp Creation Faild For Borrowing Request Id {BorroiwngRequestId}",BorrowingRequestId);
        }

        context.Otps.Add(otpCreationResult.Value);
        await context.SaveChangesAsync(ct);

        return otpCode;
    }

    public Task<Result<Success>> VerifyOtp(string email, string otp, Guid BorroiwngRequestId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}