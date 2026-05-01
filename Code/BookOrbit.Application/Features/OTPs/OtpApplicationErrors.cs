namespace BookOrbit.Application.Features.OTPs;
public static class OtpApplicationErrors
{
    public static readonly Error OtpInvalid = ApplicationCommonErrors.CustomConflict("OtpInvalid", "OtpInvalid", "The OTP is invalid.");
}