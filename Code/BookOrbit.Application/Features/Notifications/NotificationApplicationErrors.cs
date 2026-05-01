namespace BookOrbit.Application.Features.Notifications;

public static class NotificationApplicationErrors
{
    private const string ClassName = nameof(Notification);

    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(ClassName, "Id", "Id");
}
