namespace BookOrbit.Api.Contracts.Requests.Chat;

public record SendMessageRequest(
    Guid ReceiverId,
    string Content);
