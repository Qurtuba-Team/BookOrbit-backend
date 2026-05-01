namespace BookOrbit.Application.Features.Notifications.Commands.MarkAsRead;
public record MarkAsReadCommand(
    Guid StudentId,
    DateTimeOffset MaxTime) : IRequest<Result<Updated>>;