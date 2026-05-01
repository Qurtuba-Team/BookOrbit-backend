namespace BookOrbit.Application.Features.Notifications.Dtos;

public record NotificationDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid StudentId { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastModifiedUtc { get; set; }

    [JsonConstructor]
    private NotificationDto() { }

    private NotificationDto(
        Guid id,
        Guid studentId,
        string title,
        string message,
        bool isRead,
        NotificationType type,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastModifiedUtc)
    {
        Id = id;
        StudentId = studentId;
        Title = title;
        Message = message;
        IsRead = isRead;
        Type = type;
        CreatedAtUtc = createdAtUtc;
        LastModifiedUtc = lastModifiedUtc;
    }

    public static NotificationDto FromEntity(Notification notification)
        => new(
            notification.Id,
            notification.StudentId,
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.Type,
            notification.CreatedAtUtc,
            notification.LastModifiedUtc);
}
