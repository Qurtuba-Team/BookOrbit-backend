namespace BookOrbit.Domain.Common;

static public class DomainCommonErrors
{
    static public Error RequiredProp(string Class, string PropertyCode, string PropertyDescription) =>
        Error.Validation($"{Class}.{PropertyCode}.Required", $"{Class} {PropertyDescription} is required.");

    static public Error InvalidProp(string Class, string PropertyCode, string PropertyDescription, string Details = "") =>
        Error.Validation($"{Class}.{PropertyCode}.Invalid", $"{Class} {PropertyDescription} is invalid. {Details}");

    static public Error CustomValidation(string Class, string Code, string Description) =>
        Error.Validation($"{Class}.{Code}", Description);

    static public Error CustomConflict(string Class, string Code, string Description) =>
        Error.Conflict($"{Class}.{Code}", Description);

    static public Error CustomFailure(string Class, string Code, string Description)
        => Error.Failure($"{Class}.{Code}", Description);

    static public Error CustomUnAuthorized(string Class, string Code, string Description)
       => Error.Unauthorized($"{Class}.{Code}", Description);

    static public Error DateCannotBeInFuture(string Class, string PropertyCode, string PropertyDescription) =>
        Error.Validation($"{Class}.{PropertyCode}.DateCannotBeInFuture", $"{Class} {PropertyDescription} cannot be in the future.");

    static public Error DateCannotBeInPast(string Class, string PropertyCode, string PropertyDescription) =>
        Error.Validation($"{Class}.{PropertyCode}.DateCannotBeInPast", $"{Class} {PropertyDescription} cannot be in the past.");


    static public Error DateShouldBeInFuture(string Class, string PropertyCode, string PropertyDescription) =>
        Error.Validation($"{Class}.{PropertyCode}.DateShouldBeInFuture", $"{Class} {PropertyDescription} should be in the future.");

    static public Error DateShouldBeInPast(string Class, string PropertyCode, string PropertyDescription) =>
        Error.Validation($"{Class}.{PropertyCode}.DateShouldBeInPast", $"{Class} {PropertyDescription} should be in the past.");

    static public Error InvalidStateTransition(string Class, string FromState, string ToState) =>
        Error.Conflict($"{Class}.InvalidStateTransition", $"Cannot transition from {FromState} to {ToState}");

    static public Error InvalidStateTransitionSameState(string Class, string State) =>
       Error.Conflict($"{Class}.InvalidStateTransition", $"State is already {State}");
}

