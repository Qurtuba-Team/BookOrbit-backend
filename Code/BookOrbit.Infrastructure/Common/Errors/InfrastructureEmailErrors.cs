namespace BookOrbit.Infrastructure.Common.Errors;
static public class InfrastructureEmailErrors
{
    private const string ClaseName = "Infrastructure";

    static public readonly Error EmailServiceError = ApplicationCommonErrors.CustomFailure(
        ClaseName,
        "EmailServiceError",
        "An error occurred while sending the email. Please try again later.");
}