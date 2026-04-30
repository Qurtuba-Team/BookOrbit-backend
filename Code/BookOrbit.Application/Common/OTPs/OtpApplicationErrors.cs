namespace BookOrbit.Application.Common.OTPs;
public static class OtpApplicationErrors
{
    public static readonly Error OtpNotFound= ApplicationCommonErrors.NotFoundProp("OtpNotFound", "Otp Not Found", "No OTP found for the given request.");
    public static readonly Error OtpExpired = ApplicationCommonErrors.CustomConflict("OtpExpired", "OtpExpired","The OTP has expired.");
    public static readonly Error OtpInvalid = ApplicationCommonErrors.CustomConflict("OtpInvalid", "OtpInvalid", "The OTP is invalid.");
}