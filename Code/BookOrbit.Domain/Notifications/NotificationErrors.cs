namespace BookOrbit.Domain.Notifications;
static public class NotificationErrors
{
    public const string ClassName = nameof(Notification);

    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName, "Id", "Id");
    static public readonly Error MessageRequired = DomainCommonErrors.RequiredProp(ClassName, "Message", "Message");
    static public readonly Error TitleRequired = DomainCommonErrors.RequiredProp(ClassName, "Title", "Title");
    static public readonly Error StudentIdRequired = DomainCommonErrors.RequiredProp(ClassName, "StudentId", "StudentId");
    static public readonly Error InvalidNotificationType = DomainCommonErrors.InvalidProp(ClassName, "NotificationType", "Notification Type", "Invalid notification type value");
}