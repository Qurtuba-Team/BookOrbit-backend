
namespace BookOrbit.Domain.Otps;
public class Otp : ExpirableEntity
{
    public Guid TargetId { get; set; }
    public string Code { get; set; }
    public OtpType Type { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Otp() { }

    public const int Length = 6;
    public const int ExpirationInMinutes = 10;

    // 6 digits only
    private static readonly Regex CodeRegx =
   new($@"^[0-9]{{{Length},{Length}}}$", RegexOptions.Compiled);
    private Otp(Guid targetId, string code, OtpType type, Guid id,DateTimeOffset expirationDate) : base(id)
    {
        TargetId = targetId;
        Code = code;
        Type = type;
        ExpirationDateUtc = expirationDate;
    }

    static public Result<Otp> Create(Guid targetId, string code, OtpType type,DateTimeOffset currentType)
    {
        if(string.IsNullOrEmpty(code))
        {
            return OtpErrors.CodeRequired;
        }

        if(!CodeRegx.IsMatch(code))
        {
            return OtpErrors.InvalidCodeFormat;
        }

        return new Otp(targetId, code, type, Guid.NewGuid(),currentType.AddMinutes(ExpirationInMinutes));
    }
}