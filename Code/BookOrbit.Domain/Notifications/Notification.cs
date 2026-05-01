namespace BookOrbit.Domain.Notifications;
public class Notification : AuditableEntity
{
    public Guid StudentId { get; }
    public string Title { get; } = null!;
    public string Message { get; } = null!;
    public bool IsRead { get; private set; }
    public NotificationType Type { get; }

    private Notification(Guid studentId,
                        string title,
                        string message,
                        NotificationType type)
    {
        StudentId = studentId;
        Title = title;
        Message = message;
        IsRead = false;
        Type = type;
    }

    public static Result<Notification> Create(Guid studentId,
                                               string title,
                                               string message,
                                               NotificationType type)
    {
        if (studentId == Guid.Empty)
            return NotificationErrors.StudentIdRequired;

        if (string.IsNullOrWhiteSpace(title))
            return NotificationErrors.TitleRequired;

        if (string.IsNullOrWhiteSpace(message))
            return NotificationErrors.MessageRequired;

        if(!Enum.IsDefined(type))
            return NotificationErrors.InvalidNotificationType;

        return new Notification(studentId, title, message, type);
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}