namespace BookOrbit.Application.Common.Constants;

public static class NotificationCachingConstants
{
    public const string NotificationTag = "notification";

    public static string NotificationKey(Guid studentId, Guid notificationId) => $"notification:{studentId}:{notificationId}";

    public static string NotificationListKey(GetNotificationsQuery query)
        => $"notification:p={query.Page}:ps={query.PageSize}" +
           $":st={query.SearchTerm ?? "-"}" +
           $":sort={query.SortColumn}:{query.SortDirection ?? "-"}" +
           $":studentid={query.StudentId}" +
           $":isread={(query.IsRead.HasValue ? query.IsRead.Value.ToString() : "-")}" +
           $":types=[{string.Join(',', query.Types ?? [])}]";

    public const int ExpirationInMinutes = 10;
}
