using BookOrbit.Application.Features.Chat.Dtos;

namespace BookOrbit.Application.Features.Chat.Commands.SendMessage;
public record SendMessageCommand(
    Guid ReceiverId,
    string Content) : IRequest<Result<ChatMessageDto>>;
