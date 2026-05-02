using BookOrbit.Application.Features.Chat.Dtos;

namespace BookOrbit.Application.Features.Chat.Queries.GetUserChatGroups;
public class GetUserChatGroupsQueryHandler(
    ILogger<GetUserChatGroupsQueryHandler> logger,
    IAppDbContext context,
    ICurrentUser currentUser) : IRequestHandler<GetUserChatGroupsQuery, Result<PaginatedList<ChatGroupListItemDto>>>
{
    public async Task<Result<PaginatedList<ChatGroupListItemDto>>> Handle(GetUserChatGroupsQuery query, CancellationToken ct)
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

        var chatGroupsQuery = context.ChatGroups
            .AsNoTracking()
            .Include(cg => cg.Student1)
            .Include(cg => cg.Student2)
            .Where(cg => cg.Student1Id == studentId || cg.Student2Id == studentId)
            .OrderByDescending(cg => cg.CreatedAtUtc);

        int count = await chatGroupsQuery.CountAsync(ct);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);

        var items = await chatGroupsQuery
            .ApplyPagination(page, pageSize)
            .Select(cg => new ChatGroupListItemDto(
                cg.Id,
                cg.Student1Id == studentId ? cg.Student2Id : cg.Student1Id,
                cg.Student1Id == studentId ? cg.Student2!.Name.Value : cg.Student1!.Name.Value,
                cg.Student1Id == studentId
                    ? cg.Student2!.PersonalPhotoFileName
                    : cg.Student1!.PersonalPhotoFileName,
                cg.CreatedAtUtc))
            .ToListAsync(ct);

        return new PaginatedList<ChatGroupListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = MathHelper.CalculateTotalPages(count, pageSize)
        };
    }
}
