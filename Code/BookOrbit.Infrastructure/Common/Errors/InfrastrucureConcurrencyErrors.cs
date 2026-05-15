using BookOrbit.Application.Features.Students.Queries.GetStudentPublicProfileById;

namespace BookOrbit.Infrastructure.Common.Errors;

public static class InfrastrucureConcurrencyErrors
{
    private const string ClassName = "Infrastructure";

    static public readonly Error InvalidConcurrencyFormat = ApplicationCommonErrors.CustomValidation(
        ClassName,
        "InvalidConcurrencyFormat",
        "The provided concurrency token format is invalid.");

    static public readonly Error ConcurrencyTokenRequired = ApplicationCommonErrors.CustomValidation(
        ClassName,
        "ConcurrencyTokenRequired",
        "A concurrency token is required for this operation.");
}