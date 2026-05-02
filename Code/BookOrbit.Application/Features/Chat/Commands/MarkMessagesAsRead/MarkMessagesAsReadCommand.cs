namespace BookOrbit.Application.Features.Chat.Commands.MarkMessagesAsRead;
public record MarkMessagesAsReadCommand(
    Guid StudentId,
    Guid ChatGroupId) : IRequest<Result<Updated>>;
