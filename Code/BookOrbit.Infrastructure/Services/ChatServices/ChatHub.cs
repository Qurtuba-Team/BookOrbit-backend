using System.Security.Claims;
using BookOrbit.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookOrbit.Infrastructure.Services.ChatServices;

[Authorize]
public class ChatHub : Hub
{
    private readonly IAppDbContext _appDbContext;

    public ChatHub(IAppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public override async Task OnConnectedAsync()
    {
        var studentId = await GetStudentIdAsync();

        if (studentId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, studentId.Value.ToString());
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var studentId = await GetStudentIdAsync();

        if (studentId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, studentId.Value.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<Guid?> GetStudentIdAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId))
            return null;

        var studentId = await _appDbContext.Students
            .Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        return studentId == Guid.Empty ? null : studentId;
    }
}
