namespace BookOrbit.Application.Features.Chatbot.Commands.SendChatMessage;

public record SendChatMessageCommand(string Message) : IRequest<Result<ChatbotResponseDto>>;
