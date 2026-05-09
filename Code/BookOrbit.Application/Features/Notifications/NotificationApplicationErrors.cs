namespace BookOrbit.Application.Features.Notifications;

public static class NotificationApplicationErrors
{
    public static readonly Error NotFoundById = ApplicationCommonErrors.NotFoundClass(NotificationErrors.ClassName, "Id", "Id");
}
