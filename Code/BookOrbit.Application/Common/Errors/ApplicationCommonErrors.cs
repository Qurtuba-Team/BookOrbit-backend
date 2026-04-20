namespace BookOrbit.Application.Common.Errors;
static public class ApplicationCommonErrors
{
    static public Error NotFoundClass(string Class, string PropertyCode, string PropertyDescription) =>
        Error.NotFound($"{Class}.With{PropertyCode}.NotFound", $"{Class} with the specified {PropertyDescription} was not found.");
     static public Error AlreadyExists(string Class, string PropertyCode, string PropertyDescription) =>
        Error.Validation($"{Class}.{PropertyCode}.AlreadyExists", $"A {Class} with the {PropertyDescription} is already exists.");

    static public Error NotFoundProp(string Class, string PropertyCode, string PropertyDescription) =>
        Error.NotFound($"{Class}.{PropertyCode}.NotFound", $"{PropertyDescription} was not found in the system.");

    static public Error CustomConflict(string Class, string Code, string Description) => DomainCommonErrors.CustomConflict(Class, Code, Description);
    static public Error CustomValidation(string Class, string Code, string Description) => DomainCommonErrors.CustomValidation(Class, Code, Description);
    static public Error CustomFailure (string Class, string Code, string Description) => DomainCommonErrors.CustomFailure(Class, Code, Description);
    static public Error CustomUnauthorized(string Class, string Code, string Description) => DomainCommonErrors.CustomUnAuthorized(Class, Code, Description);
}