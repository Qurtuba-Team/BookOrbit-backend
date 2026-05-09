namespace BookOrbit.Infrastructure.Common.Errors;
static public class InfrastructureOutboxMessageErrors
{
    private const string ClaseName = "Infrastructure";

    static public readonly Error SerializationFailed = ApplicationCommonErrors.CustomFailure(
        ClaseName,
        "SerializationFailed",
        "Failed to serialize or deserialize the outbox message payload.");

}