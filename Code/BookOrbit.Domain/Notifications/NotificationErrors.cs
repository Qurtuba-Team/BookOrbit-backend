namespace BookOrbit.Domain.Notifications;
static public class NotificationErrors
{
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(nameof(Notification), "Id", "Id");
    static public readonly Error MessageRequired = DomainCommonErrors.RequiredProp(nameof(Notification), "Message", "Message");
    static public readonly Error TitleRequired = DomainCommonErrors.RequiredProp(nameof(Notification), "Title", "Title");
    static public readonly Error StudentIdRequired = DomainCommonErrors.RequiredProp(nameof(Notification), "StudentId", "StudentId");
    static public readonly Error InvalidNotificationType = DomainCommonErrors.InvalidProp(nameof(Notification), "NotificationType", "Notification Type", "Invalid notification type value");
}