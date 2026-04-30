namespace BookOrbit.Domain.Otps;
static public class OtpErrors
{
    private const string ClassName = nameof(OtpErrors);

    public static readonly Error CodeRequired = DomainCommonErrors.RequiredProp(ClassName, "OtpCode", "Otp Code");
    public static readonly Error InvalidCodeFormat = DomainCommonErrors.CustomValidation(ClassName, "OtpCode", "Otp Code should be 6 digits");


}