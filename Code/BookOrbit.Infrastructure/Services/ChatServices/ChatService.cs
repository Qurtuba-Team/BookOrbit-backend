using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Common.Interfaces.ChatServices;
using BookOrbit.Domain.ChatGroups;
using BookOrbit.Domain.ChatMessages;
using BookOrbit.Domain.Students;
using Microsoft.EntityFrameworkCore;
using BookOrbit.Domain.Common.Results;

namespace BookOrbit.Infrastructure.Services.ChatServices;

public class ChatService : IChatService
{
    private readonly IAppDbContext _appDbContext;
    private readonly IRealTimeService _realTimeService;

    public ChatService(IAppDbContext appDbContext, IRealTimeService realTimeService)
    {
        _appDbContext = appDbContext;
        _realTimeService = realTimeService;
    }

    public async Task<Result<ChatGroup>> GetOrCreateChatGroupAsync(Guid student1Id, Guid student2Id, CancellationToken cancellationToken = default)
    {
        var firstStudentId = student1Id.CompareTo(student2Id) < 0 ? student1Id : student2Id;
        var secondStudentId = student1Id.CompareTo(student2Id) < 0 ? student2Id : student1Id;

        var chatGroup = await _appDbContext.ChatGroups
            .FirstOrDefaultAsync(cg => cg.Student1Id == firstStudentId && cg.Student2Id == secondStudentId, cancellationToken);

        if (chatGroup != null)
        {
            return chatGroup;
        }

        var result = ChatGroup.Create(Guid.NewGuid(), firstStudentId, secondStudentId);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        await _appDbContext.ChatGroups.AddAsync(result.Value, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        return result.Value;
    }

    public async Task<Result<IReadOnlyList<ChatGroup>>> GetUserChatGroupsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var chatGroups = await _appDbContext.ChatGroups
            .Include(cg => cg.Student1)
            .Include(cg => cg.Student2)
            .Where(cg => cg.Student1Id == studentId || cg.Student2Id == studentId)
            .ToListAsync(cancellationToken);

        return chatGroups;
    }

    public async Task<Result<IReadOnlyList<ChatMessage>>> GetChatHistoryAsync(Guid chatGroupId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var messages = await _appDbContext.ChatMessages
            .Where(cm => cm.ChatGroupId == chatGroupId)
            .OrderByDescending(cm => cm.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return messages;
    }

    public async Task<Result<Updated>> MarkMessagesAsReadAsync(Guid chatGroupId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var unreadMessages = await _appDbContext.ChatMessages
            .Where(cm => cm.ChatGroupId == chatGroupId && cm.SenderId != studentId && !cm.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadMessages.Count == 0)
        {
            return Result.Updated;
        }

        foreach (var message in unreadMessages)
        {
            message.MarkAsRead();
            await _realTimeService.NotifyMessageReadAsync(message.SenderId, message.Id, cancellationToken);
        }

        await _appDbContext.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }

    public async Task<Result<ChatMessage>> SaveMessageAsync(Guid senderId, Guid receiverId, string content, CancellationToken cancellationToken = default)
    {
        var chatGroupResult = await GetOrCreateChatGroupAsync(senderId, receiverId, cancellationToken);
        
        if (chatGroupResult.IsFailure)
        {
            return chatGroupResult.TopError;
        }

        var chatGroup = chatGroupResult.Value;

        var messageResult = ChatMessage.Create(Guid.NewGuid(), content, senderId, chatGroup.Id);

        if (messageResult.IsFailure)
        {
            return messageResult.TopError;
        }

        await _appDbContext.ChatMessages.AddAsync(messageResult.Value, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        await _realTimeService.SendMessageToStudentAsync(receiverId, messageResult.Value, cancellationToken);

        return messageResult.Value;
    }
}
