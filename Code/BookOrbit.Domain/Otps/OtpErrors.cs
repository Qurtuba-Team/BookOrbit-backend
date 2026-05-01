namespace BookOrbit.Domain.Otps;
static public class OtpErrors
{
    private const string ClassName = nameof(OtpErrors);

    public static readonly Error CodeRequired = DomainCommonErrors.RequiredProp(ClassName, "OtpCode", "Otp Code");
    public static readonly Error InvalidCodeFormat = DomainCommonErrors.CustomValidation(ClassName, "OtpCode", "Otp Code should be 6 digits");
    public static readonly Error OtpAlreadyUsed = DomainCommonErrors.CustomValidation(ClassName, "Otp", "Otp is already used");
    public static readonly Error OtpExpired = DomainCommonErrors.CustomValidation(ClassName, "Otp", "Otp is expired");

}