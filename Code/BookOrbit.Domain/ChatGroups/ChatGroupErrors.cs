namespace BookOrbit.Domain.ChatGroups;

public static class ChatGroupErrors
{
    private const string ClassName = nameof(ChatGroup);

    public static readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    public static readonly Error Student1IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Student1Id", "Student 1 Id");
    public static readonly Error Student2IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Student2Id", "Student 2 Id");
    public static readonly Error SameStudent = DomainCommonErrors.CustomValidation(ClassName, "SameStudent", "A student cannot create a chat group with themselves.");
}
