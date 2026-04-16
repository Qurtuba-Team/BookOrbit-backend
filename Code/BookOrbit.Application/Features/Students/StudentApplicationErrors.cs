namespace BookOrbit.Application.Features.Students;

static public class StudentApplicationErrors
{
    private const string ClassName = nameof(Student);

    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
    static public readonly Error NotFoundByUserId = ApplicationCommonErrors.NotFoundClass(ClassName, "UserId", "User Id");
    static public readonly Error EmailAlreadyExists = ApplicationCommonErrors.AlreadyExists(ClassName, "Univestymail", "Univesty Mail");
    static public readonly Error TelegramUserIdAlreadyExists = ApplicationCommonErrors.AlreadyExists(ClassName, "TelegramUserId", "Telegram User Id");
    static public readonly Error PhoneNumberAlreadyExists = ApplicationCommonErrors.AlreadyExists(ClassName, "PhoneNumber", "Phone Number");
    static public readonly Error PersonalImageNotFound = ApplicationCommonErrors.NotFoundProp(ClassName, "PersonalImage", "Personal Image");
    static public readonly Error StateTransitionFailedEmailNotConfirmed = ApplicationCommonErrors.CustomConflict(ClassName, "StateTransitionFailedEmailNotConfirmed", "Cannot Transit Student State , Email Is Not Confirmed");
    static public readonly Error StateIsNotActive = ApplicationCommonErrors.CustomConflict(ClassName, "StudentIsNotActive", "Cannot Performe The Operation , Student Is Not Active");
}

