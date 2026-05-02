namespace BookOrbit.Application.Common.Interfaces.ChatServices;

public interface IRealTimeService
{
    Task SendMessageToStudentAsync(Guid studentId, ChatMessage message, CancellationToken cancellationToken = default);
    Task NotifyMessageReadAsync(Guid studentId, Guid messageId, CancellationToken cancellationToken = default);
}
