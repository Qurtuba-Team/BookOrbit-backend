
namespace BookOrbit.Domain.Otps;
public class Otp : ExpirableEntity
{
    public Guid TargetId { get; set; }
    public string Code { get; set; }
    public OtpType Type { get; set; }
    public bool IsUsed { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Otp() { }

    public const int Length = 6;
    public const int ExpirationInMinutes = 10;

    // 6 digits only
    private static readonly Regex CodeRegx =
   new($@"^[0-9]{{{Length},{Length}}}$", RegexOptions.Compiled);
    private Otp(Guid id,Guid targetId, string code, OtpType type,DateTimeOffset expirationDate) : base(id)
    {
        TargetId = targetId;
        Code = code;
        Type = type;
        ExpirationDateUtc = expirationDate;
        IsUsed = false;
    }

    static public Result<Otp> Create(Guid id,Guid targetId, string code, OtpType type,DateTimeOffset currentTime)
    {
        if(string.IsNullOrEmpty(code))
        {
            return OtpErrors.CodeRequired;
        }

        if(!CodeRegx.IsMatch(code))
        {
            return OtpErrors.InvalidCodeFormat;
        }

        return new Otp(id,targetId, code, type,currentTime.AddMinutes(ExpirationInMinutes));
    }

    public Result<Updated> Use(DateTimeOffset currentTime)
    {
        if (IsUsed)
        {
            return OtpErrors.OtpAlreadyUsed;
        }
        if (currentTime > ExpirationDateUtc)
        {
            return OtpErrors.OtpExpired;
        }
        IsUsed = true;
        return  Result.Updated;
    }

}