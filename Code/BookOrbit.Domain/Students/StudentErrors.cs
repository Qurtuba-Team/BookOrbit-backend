
namespace BookOrbit.Domain.Students;

static public class StudentErrors
{
    private const string ClassName = nameof(Student);

    #region General
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName,"Id","Id");
    static public readonly Error UserIdRequired = DomainCommonErrors.RequiredProp(ClassName,"UserId","User Id");
    static public readonly Error NameRequired = DomainCommonErrors.RequiredProp(ClassName,"Name","Name");
    static public readonly Error InvalidName = DomainCommonErrors.InvalidProp(ClassName, "Name", "Name", $"It must be between {StudentName.MinLength} and {StudentName.MaxLength} English characters");
    static public readonly Error InvalidJoinDate = DomainCommonErrors.InvalidProp(ClassName, "JoinDate", "Join Date", "It cannot be before creation date request");
    static public readonly Error InvalidState = DomainCommonErrors.InvalidProp(ClassName, "StudentState", "Student State", $"Invalid state value");
    static public readonly Error AtLeastOneCommunicationMethod = DomainCommonErrors.CustomValidation(ClassName, "AtLeastOneCommunicationMethod", "At least one communication method (Phone Number or Telegram Username) must be provided.");
    static public readonly Error PersonalImageRequired = DomainCommonErrors.RequiredProp(ClassName,"PersonalImageId","Personal Image");
    static public readonly Error CannotUpdateABannedStudent = DomainCommonErrors.CustomConflict(ClassName, "CannotUpdateABannedStudent", "Cannot update student while state is banned");
    static public readonly Error InsufficientPoints = DomainCommonErrors.CustomValidation(ClassName, "InsufficientPoints", "The student does not have enough points to perform this action.");
    #endregion

    #region Mail
    static public readonly Error UniversityMailRequired = DomainCommonErrors.RequiredProp(ClassName,"UniversityMail","University Mail");
    static public readonly Error InvalidUniversityMail = DomainCommonErrors.InvalidProp(ClassName, "UniversityMail", "University Mail", "It must be a valid email address , and end with @std.mans.edu.eg less than 320 characters");
    #endregion

    #region Logic

    public static Error InvalidStateTransition(StudentState currentState, StudentState newState)
    {
        if(currentState == newState)
            return DomainCommonErrors.InvalidStateTransitionSameState(ClassName, currentState.ToString());
        else
            return DomainCommonErrors.InvalidStateTransition(ClassName, currentState.ToString(), newState.ToString());
    }

    #endregion
}

