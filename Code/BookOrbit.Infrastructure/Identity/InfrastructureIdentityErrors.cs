namespace BookOrbit.Infrastructure.Identity;
static public class InfrastructureIdentityErrors
{
    private const string ClassName = "User";

    static public readonly Error UserNotFoundByEmail = ApplicationCommonErrors.NotFoundClass(ClassName, "Email", "Email");
    static public readonly Error UserNotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
    static public readonly Error EmailNotConfirmed = ApplicationCommonErrors.CustomConflict(ClassName, "UserEmailNotConfirmed", "This user hasnt confirm his email yet.");
    static public readonly Error EmailAlreadyConfirmed = ApplicationCommonErrors.CustomConflict(ClassName, "UserEmailAlreadyConfirmed", "This user has confirmed his email already.");
    static public readonly Error InvalidLoginAttempt = DomainCommonErrors.CustomUnAuthorized(ClassName, "FaildLoginAttempt", "Email or Password are incorrect.");
    static public readonly Error UserCreationFaild = ApplicationCommonErrors.CustomFailure(ClassName, "UserCreationFaild", "Failed To Create User");
    static public readonly Error UserDeletionFailed = ApplicationCommonErrors.CustomFailure(ClassName, "UserDeletionFaild", "Faild to Delete User");
    static public readonly Error InvalidEmailConfirmationToken = ApplicationCommonErrors.CustomValidation(ClassName, "InvalidEmailConfirmationToken","Email confirmation token is not valid");
    static public readonly Error UserNotAuthenticated = ApplicationCommonErrors.CustomUnauthorized(ClassName, "UserNotAuthenticated", "User is not authenticated.");
}