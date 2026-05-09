namespace BookOrbit.Application.Features.Students;

static public class StudentApplicationErrors
{
    static public readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(StudentErrors.ClassName, "Id", "Id");
    static public readonly Error NotFoundByUserId = ApplicationCommonErrors.NotFoundClass(StudentErrors.ClassName, "UserId", "User Id");
    static public readonly Error EmailAlreadyExists = ApplicationCommonErrors.AlreadyExists(StudentErrors.ClassName, "Univestymail", "Univesty Mail");
    static public readonly Error TelegramUserIdAlreadyExists = ApplicationCommonErrors.AlreadyExists(StudentErrors.ClassName, "TelegramUserId", "Telegram User Id");
    static public readonly Error PhoneNumberAlreadyExists = ApplicationCommonErrors.AlreadyExists(StudentErrors.ClassName, "PhoneNumber", "Phone Number");
    static public readonly Error PersonalImageNotFound = ApplicationCommonErrors.NotFoundProp(StudentErrors.ClassName, "PersonalImage", "Personal Image");
    static public readonly Error StateTransitionFailedEmailNotConfirmed = ApplicationCommonErrors.CustomConflict(StudentErrors.ClassName, "StateTransitionFailedEmailNotConfirmed", "Cannot Transit Student State , Email Is Not Confirmed");
    static public readonly Error StateIsNotActive = ApplicationCommonErrors.CustomConflict(StudentErrors.ClassName, "StudentIsNotActive", "Cannot Performe The Operation , Student Is Not Active");
    static public readonly Error NotFoundByEmail = ApplicationCommonErrors.NotFoundClass(StudentErrors.ClassName, "Email", "Email");

}

