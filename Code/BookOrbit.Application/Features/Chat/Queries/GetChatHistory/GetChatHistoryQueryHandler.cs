namespace BookOrbit.Application.Features.Chat.Queries.GetChatHistory;
public class GetChatHistoryQueryHandler(
    ILogger<GetChatHistoryQueryHandler> logger,
    IAppDbContext context,
    ICurrentUser currentUser) : IRequestHandler<GetChatHistoryQuery, Result<PaginatedList<ChatMessageDto>>>
{
    public async Task<Result<PaginatedList<ChatMessageDto>>> Handle(GetChatHistoryQuery query, CancellationToken ct)
    {
        var userId = currentUser.Id;

        if (string.IsNullOrEmpty(userId))
            return ChatApplicationErrors.StudentNotFound;

        var studentId = await context.Students
            .Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(ct);

        if (studentId == Guid.Empty)
        {
            logger.LogWarning("Student not found for UserId: {UserId}", userId);
            return ChatApplicationErrors.StudentNotFound;
        }

        var chatGroup = await context.ChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.Id == query.ChatGroupId, ct);

        if (chatGroup is null)
        {
            logger.LogWarning("ChatGroup {ChatGroupId} not found.", query.ChatGroupId);
            return ChatApplicationErrors.ChatGroupNotFoundById;
        }

        if (chatGroup.Student1Id != studentId && chatGroup.Student2Id != studentId)
        {
            logger.LogWarning("Student {StudentId} is not part of ChatGroup {ChatGroupId}.", studentId, query.ChatGroupId);
            return ChatApplicationErrors.UserNotPartOfChatGroup;
        }

        var messagesQuery = context.ChatMessages
            .AsNoTracking()
            .Where(cm => cm.ChatGroupId == query.ChatGroupId)
            .OrderByDescending(cm => cm.CreatedAtUtc);

        int count = await messagesQuery.CountAsync(ct);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);

        var items = await messagesQuery
            .ApplyPagination(page, pageSize)
            .Select(cm => new ChatMessageDto(
                cm.Id,
                cm.Content,
                cm.SenderId,
                cm.ChatGroupId,
                cm.IsRead,
                cm.CreatedAtUtc))
            .ToListAsync(ct);

        return new PaginatedList<ChatMessageDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }
}
