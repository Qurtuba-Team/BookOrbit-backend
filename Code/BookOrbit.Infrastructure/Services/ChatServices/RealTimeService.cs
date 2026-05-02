using BookOrbit.Application.Common.Interfaces.ChatServices;
using BookOrbit.Application.Features.Chat.Dtos;
using BookOrbit.Domain.ChatMessages;
using Microsoft.AspNetCore.SignalR;

namespace BookOrbit.Infrastructure.Services.ChatServices;

public class RealTimeService(IHubContext<ChatHub> hubContext) : IRealTimeService
{
    private readonly IHubContext<ChatHub> _hubContext = hubContext;

    public async Task SendMessageToStudentAsync(Guid studentId, ChatMessage message, CancellationToken cancellationToken = default)
    {
        var payload = ChatMessageDto.FromEntity(message);

        await _hubContext.Clients.Group(studentId.ToString()).SendAsync("ReceiveMessage", payload, cancellationToken);
    }

    public async Task NotifyMessageReadAsync(Guid studentId, Guid messageId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(studentId.ToString()).SendAsync("MessageRead", new { MessageId = messageId }, cancellationToken);
    }
}
