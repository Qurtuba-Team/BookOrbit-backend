namespace BookOrbit.Application.Features.Notifications.Dtos;

public record NotificationListItemDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid StudentId { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonConstructor]
    private NotificationListItemDto() { }

    private NotificationListItemDto(
        Guid id,
        Guid studentId,
        string title,
        string message,
        bool isRead,
        NotificationType type,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        StudentId = studentId;
        Title = title;
        Message = message;
        IsRead = isRead;
        Type = type;
        CreatedAtUtc = createdAtUtc;
    }

    public static Expression<Func<Notification, NotificationListItemDto>> Projection =>
        n => new NotificationListItemDto(
            n.Id,
            n.StudentId,
            n.Title,
            n.Message,
            n.IsRead,
            n.Type,
            n.CreatedAtUtc);
}
